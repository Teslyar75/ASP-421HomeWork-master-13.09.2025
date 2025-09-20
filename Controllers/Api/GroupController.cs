using ASP_421.Data;
using ASP_421.Models.Shop.Api;
using ASP_421.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_421.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(
        IStorageService storageService, DataContext dataContext
        ) : ControllerBase
    {
        
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public object AllGroups()
        {
            try
            {
                var groups = _dataContext.ProductGroups
                    .Where(g => g.DeletedAt == null)
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Description,
                        g.Slug,
                        g.ImageUrl,
                        ProductsCount = g.Products.Count(p => p.DeletedAt == null)
                    })
                    .ToList();

                return new
                {
                    Status = "Ok",
                    Data = groups,
                    Count = groups.Count
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Fail",
                    ErrorMessage = ex.Message
                };
            }
        }

        [HttpGet("test")]
        public object Test()
        {
            return new
            {
                Status = "Ok",
                Message = "API працює!",
                Timestamp = DateTime.Now
            };
        }

        [HttpGet("db-test")]
        public object TestDatabase()
        {
            try
            {
                // Проверяем подключение к БД
                var canConnect = _dataContext.Database.CanConnect();
                var groupsCount = _dataContext.ProductGroups.Count();
                
                return new
                {
                    Status = "Ok",
                    Message = "База даних працює!",
                    CanConnect = canConnect,
                    GroupsCount = groupsCount,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Fail",
                    Message = "Помилка підключення до БД",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    Timestamp = DateTime.Now
                };
            }
        }


        [HttpPost]
        public object CreateGroup(ShopApiGroupFormModel formModel)
        {
            // Детальная валидация модели формы
            var validationErrors = new Dictionary<string, string>();

            // Валидация названия группы
            if (string.IsNullOrWhiteSpace(formModel.Name))
            {
                validationErrors["Name"] = "Назва групи не може бути порожньою";
            }
            else if (formModel.Name.Length < 2)
            {
                validationErrors["Name"] = "Назва групи повинна містити мінімум 2 символи";
            }
            else if (formModel.Name.Length > 100)
            {
                validationErrors["Name"] = "Назва групи не може бути довшою за 100 символів";
            }

            // Валидация описания
            if (!string.IsNullOrWhiteSpace(formModel.Description) && formModel.Description.Length > 500)
            {
                validationErrors["Description"] = "Опис групи не може бути довшим за 500 символів";
            }

            // Валидация slug
            if (string.IsNullOrWhiteSpace(formModel.Slug))
            {
                validationErrors["Slug"] = "Slug не може бути порожнім";
            }
            else if (formModel.Slug.Length < 2)
            {
                validationErrors["Slug"] = "Slug повинен містити мінімум 2 символи";
            }
            else if (formModel.Slug.Length > 50)
            {
                validationErrors["Slug"] = "Slug не може бути довшим за 50 символів";
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(formModel.Slug, @"^[a-z0-9\-]+$"))
            {
                validationErrors["Slug"] = "Slug може містити тільки малі літери, цифри та дефіси";
            }
            else if (_dataContext.ProductGroups.Any(g => g.Slug == formModel.Slug && g.DeletedAt == null))
            {
                validationErrors["Slug"] = "Група з таким slug вже існує";
            }

            // Валидация изображения
            if (formModel.Image == null || formModel.Image.Length == 0)
            {
                validationErrors["Image"] = "Необхідно вибрати зображення";
            }
            else if (formModel.Image.Length > 5 * 1024 * 1024) // 5MB
            {
                validationErrors["Image"] = "Розмір зображення не може перевищувати 5MB";
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var extension = Path.GetExtension(formModel.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    validationErrors["Image"] = $"Дозволені формати: {string.Join(", ", allowedExtensions)}";
                }
            }

            // Если есть ошибки валидации, возвращаем их
            if (validationErrors.Count > 0)
            {
                return new
                {
                    Status = "ValidationError",
                    Errors = validationErrors
                };
            }
            
            try
            {
                // Логируем данные для отладки
                System.Diagnostics.Debug.WriteLine($"Creating group: Name={formModel.Name}, Slug={formModel.Slug}");
                
                // Сохраняем изображение
                string imageUrl;
                try
                {
                    imageUrl = _storageService.Save(formModel.Image);
                    System.Diagnostics.Debug.WriteLine($"Image saved: {imageUrl}");
                }
                catch (Exception imgEx)
                {
                    return new
                    {
                        Status = "Fail",
                        ErrorMessage = "Помилка збереження зображення: " + imgEx.Message,
                        Details = imgEx.InnerException?.Message
                    };
                }
                
                // Создаем группу
                var newGroup = new Data.Entities.ProductGroup()
                {
                    Id = Guid.NewGuid(),
                    Name = formModel.Name.Trim(),
                    Description = formModel.Description?.Trim(),
                    Slug = formModel.Slug.Trim().ToLowerInvariant(),
                    ImageUrl = imageUrl
                };
                
                _dataContext.ProductGroups.Add(newGroup);
                System.Diagnostics.Debug.WriteLine("Group added to context");
                
                _dataContext.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Changes saved to database");
                
                return new
                {
                    Status = "Ok",
                    Message = "Нова група створена",
                    GroupId = newGroup.Id
                };
            }
            catch (Exception ex)
            {
                // Логируем детальную ошибку для отладки
                System.Diagnostics.Debug.WriteLine($"Error creating group: {ex}");
                
                return new
                {
                    Status = "Fail",
                    ErrorMessage = "Помилка збереження: " + ex.Message,
                    Details = ex.InnerException?.Message // Добавляем детали для отладки
                };
            }
        }
    }
}
/*
 * API для роботи з групами товарів
 * Aplication Programming Interface
 * Взаємодія с іншими програмами
 *                                          
 *                   Program ----- api ---- Programms(інші програми)
 *                   /      \
 *                  / API    \ 
 *         Application   Application
 *         (web site)     (mobile app)
 *         
 * передача даних між програмами проходить у форматі JSON
 * Відмінності між контроллерами MVC і ApiController.
 *                       MVC                          ApiController
 * Адресація           /Ctrl/Action                   /api/Ctrl/   
 * Вибір дії         за  методом  Action            за  методом   HTTP метод    
 * Повернення       View, Redirect, JSON            object що автоматично перетворюється на JSON
 * "Вага"             більша                       менша
 */