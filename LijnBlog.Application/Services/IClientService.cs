using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LijnBlog.Application.Services;

public interface IClientService
{
    public Task CreateAsync();

    public Task GetAllAsync();

    public Task GetById(Guid ClientId);

    public Task DeleteByIdAsync(Guid ClientId);

    public Task UpdateById(Guid ClientId);
}
