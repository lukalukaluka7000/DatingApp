using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MAX_PAGE_SIZE = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 3;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (MAX_PAGE_SIZE > value) ? value : MAX_PAGE_SIZE;
        }
    }
}
