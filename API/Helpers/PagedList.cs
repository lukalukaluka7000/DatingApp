using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int pageSize, int totalNumItems, int currentPage)
        {
            TotalPages = this.getTotalPages(totalNumItems, pageSize);
            PageSize = pageSize;
            TotalNumItems = totalNumItems;
            CurrentPage = currentPage;
            AddRange(items);
        }
        private int getTotalPages(int numItems, int pageSize)
        {
            return (int)Math.Ceiling((numItems / (double)pageSize));
        }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalNumItems { get; set; }
        public int CurrentPage { get; set; }

        //ovdi stavljan sta primim iz querya
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, 
            int pageNumber, int pageSize)
        {
            var calculatedItems = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(calculatedItems, pageSize, await source.CountAsync(), pageNumber);
        }
    }
}
