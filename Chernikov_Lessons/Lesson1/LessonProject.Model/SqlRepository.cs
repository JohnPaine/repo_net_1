﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace LessonProject.Model
{
    public partial class SqlRepository : IRepository
    {
        [Inject]
        public LessonProjectDbDataContext Db { get; set; }
    }
}