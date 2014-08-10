﻿using System.Collections.Generic;

namespace OpsBI.Importer.Models
{
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Result = new List<T>();
        }

        public IList<T> Result { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
    }
}
