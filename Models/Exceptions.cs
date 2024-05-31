using Repos;
namespace Exceptions
{
    /// <summary>
    /// Исключение для несуществующих объявлений, файлов, пользователей
    /// </summary>
    public class DoesNotExistException : Exception
    {
        public DoesNotExistException(Type entityType)
        {
            if (entityType == typeof(Advertisement))
                Message = "Advertisement does not exists";
            else if (entityType == typeof(User))
                Message = "User does not exists";
            else if (entityType == typeof(File))
                Message = "File does not exists";
            else if (entityType == typeof(UserProfile))
                Message = "User profile does not exists";
            else throw new Exception();
        }
        public new string Message;
    }
    /// <summary>
    /// Исключение для некорректного номера страницы
    /// </summary>
    public class InvalidPageException : Exception
    {
        public string Message = "Page number is invalid or does not exist";
    }
    /// <summary>
    /// Исключение для неподходящего формата файла (выбрасывается, если отправлено не фото)
    /// </summary>
    public class InvalidFileFormatException : Exception
    {
        public string Message = "Sent file is not a picture";
    }
    /// <summary>
    /// Исключение для неотправленного файла (шутки ради дал возможность пользователю отправлять пустой файл, чтобы можно было вернуть ошибку 418 - I'm a teapot)
    /// </summary>
    public class EmptyFileException : Exception
    {
        public string Message = "File was not sent";
    }
    /// <summary>
    /// Исключение для попытки превысить лимит объявлений для одного пользователя (не действует на админов)
    /// </summary>
    public class TooManyAdsException : Exception
    {
        public string Message = "You can not create more ads. Try to delete old one.";
    }
}
