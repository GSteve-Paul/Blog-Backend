﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LijnBlog.Application.Cache;

public interface ICacheConnectionFactory
{
    IDatabase CreateConnectionAsync();
}
