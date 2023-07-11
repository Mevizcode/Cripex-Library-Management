using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
    [NotMapped]
    public class Pagination
    {
        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public string ActionName { get; private set; }
        public string ControllerName { get; private set; }

        public Pagination()
        {
        }

        public Pagination(string actionName, string controllerName, int totalItems, int page, int pageSize = 10)
        {
            int totalPages = (int)Math.Ceiling(totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 2; //5
            int endPage = currentPage + 1; //4

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
            ActionName = actionName;
            ControllerName = controllerName;

        }
    }
}
