using ASP_421.Data;
using ASP_421.Models.User;
using ASP_421.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace ASP_421.Controllers
{
    public class UserController(
        DataContext dataContext, 
        IKdfService kdfService) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        const String RegisterKey = "RegisterFormModel";

        public JsonResult SignIn()
        {
            // Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==

            String header = // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
                HttpContext.Request.Headers.Authorization.ToString();
            if (String.IsNullOrEmpty(header))
            {
                return Json(new { 
                    Status = 401, 
                    Data = "Missing Authorization header" });
            }
            String scheme = "Basic ";
            if(!header.StartsWith(scheme))
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Invalid scheme. Required: " + scheme 
                });
            }
            String credentials =   // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
                header[scheme.Length..];

            // Проверяем, что credentials не пусты
            if (String.IsNullOrEmpty(credentials))
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Empty credentials"
                });
            }

            String userPass;
            try
            {
                // Декодирование credentials по Base64 должно состояться успешно
                userPass = Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch (FormatException)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Invalid Base64 format"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Decoding error: " + ex.Message
                });
            }

            // Декодированные данные должны содержать ":" и делиться на две части
            if (!userPass.Contains(':'))
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Invalid credentials format - missing separator"
                });
            }

            String[] parts = userPass.Split(':', 2);
            
            // После разделения каждая из частей не должна быть пуста
            if (parts.Length != 2 || String.IsNullOrEmpty(parts[0]) || String.IsNullOrEmpty(parts[1]))
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Invalid credentials - empty login or password"
                });
            }

            String login = parts[0];     // Aladdin
            String password = parts[1];  // open sesame

            var userAccess = _dataContext
                .UserAccesses
                .AsNoTracking()           // не моніторити зміни -- тільки читання
                .Include(ua => ua.User)   // заповнення навігаційної властивості
                .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Виникла помилка, спробуйте ще раз."
                });
            }
            if(_kdfService.Dk(password, userAccess.Salt) != userAccess.Dk)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "Credentials rejected"
                });
            }
            // користувач пройшов автентифікацію, зберігаємо у сесії
            HttpContext.Session.SetString(
                "SignIn",
                JsonSerializer.Serialize(userAccess));

            return Json(new
            {
                Status = 200,
                Data = "Authorized",
                UserName = userAccess.User.Name
            });
        }

        public IActionResult SignUp()
        {
            UserSignupViewModel viewModel = new();
            
            if(HttpContext.Session.Keys.Contains(RegisterKey))
            {
                UserSignupFormModel formModel =
                    JsonSerializer.Deserialize<UserSignupFormModel>(
                        HttpContext.Session.GetString(RegisterKey)!)!;

                viewModel.FormModel = formModel;
                viewModel.ValidationErrors = ValidateSignupForm(formModel);

                if(viewModel.ValidationErrors.Count == 0)
                {
                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel.Name,
                        Email = formModel.Email,
                        Birthdate = formModel.Birthday,
                        RegisteredAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    String salt = Guid.NewGuid().ToString();
                    Data.Entities.UserAccess userAccess = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = "Guest",
                        Login = formModel.Login,
                        Salt = salt,
                        Dk = _kdfService.Dk(formModel.Password, salt),
                    };
                    _dataContext.Users.Add(user);
                    _dataContext.UserAccesses.Add(userAccess);
                    try
                    {
                        _dataContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        viewModel
                            .ValidationErrors[nameof(viewModel.ValidationErrors)] = ex.Message;
                    }
                }

                HttpContext.Session.Remove(RegisterKey);
            }
            return View(viewModel);
        }


        /// Обрабатывает POST запрос на регистрацию пользователя
        /// Сохраняет данные формы в сессии и перенаправляет на форму для показа ошибок

        [HttpPost]
        public RedirectToActionResult Register(UserSignupFormModel formModel)
        {
            // Сохраняем данные формы в сессии в формате JSON
            HttpContext.Session.SetString(
                RegisterKey,
                JsonSerializer.Serialize(formModel));

            // Перенаправляем на форму для показа ошибок валидации
            return RedirectToAction(nameof(SignUp));
        }


        /// Валидирует данные формы регистрации пользователя
        /// Возвращает словарь с ошибками валидации для каждого поля

        private Dictionary<String, String> ValidateSignupForm(UserSignupFormModel formModel)
        {
            Dictionary<String, String> res = new();

            // Валідація імені
            if (String.IsNullOrEmpty(formModel.Name))
            {
                res[nameof(formModel.Name)] = "Ім'я не може бути порожнім";
            }
            else if (!char.IsUpper(formModel.Name[0]))
            {
                res[nameof(formModel.Name)] = "Ім'я повинно починатися з великої літери";
            }
            else if (!formModel.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                res[nameof(formModel.Name)] = "Ім'я може містити тільки літери та пробіли";
            }

            // Валідація Email
            if (String.IsNullOrEmpty(formModel.Email))
            {
                res[nameof(formModel.Email)] = "E-mail не може бути порожнім";
            }
            else if (!IsValidEmail(formModel.Email))
            {
                res[nameof(formModel.Email)] = "Введіть правильний формат E-mail";
            }

            // Валідація логіну
            if (formModel.Login?.Contains(':') ?? false)
            {
                res[nameof(formModel.Login)] = "У логіні не допускається ':' (двокрапка)";
            }

            // Валідація паролю
            if (String.IsNullOrEmpty(formModel.Password))
            {
                res[nameof(formModel.Password)] = "Пароль не може бути порожнім";
            }
            else if (formModel.Password.Length < 8)
            {
                res[nameof(formModel.Password)] = "Пароль повинен містити мінімум 8 символів";
            }
            else if (!formModel.Password.Any(char.IsUpper))
            {
                res[nameof(formModel.Password)] = "Пароль повинен містити хоча б одну велику літеру";
            }
            else if (!formModel.Password.Any(char.IsDigit))
            {
                res[nameof(formModel.Password)] = "Пароль повинен містити хоча б одну цифру";
            }

            // Валідація повтору паролю
            if (formModel.Password != formModel.Repeat)
            {
                res[nameof(formModel.Repeat)] = "Паролі не збігаються";
            }

            return res;
        }


        /// Валидирует корректность email адреса
        /// Использует встроенный класс MailAddress для проверки формата

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}

/*  Browser           НЕПРАВИЛЬНО               Server
 * POST name=User ----------------------------->  
 *     <---------------------------------------- HTML
 *  Оновити: POST name=User -------------------> ?Conflict - повторні дані
 */

/*  Browser             ПРАВИЛЬНО               Server
 * POST /Register name=User -------------------> Зберігає дані (у сесії)
 *     <------------------302------------------- Redirect /SignUp
 * GET /SignUp  -------------------------------> Відновлює та оброблює дані 
 *     <------------------200------------------- HTML
 * Оновити: GET /SignUp -----------------------> Немає конфлікту    
 */

/* Д.З. Реалізувати повну валідацію даних форми реєстрації користувача:
 * - правильність імені (починається з великої літери, не містить спецзнаки тощо)
 * - правильність E-mail
 * - вимогу до паролю (довжина, склад)
 * Вивести відповідні повідмолення на формі
 */