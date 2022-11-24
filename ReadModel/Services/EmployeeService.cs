﻿using ReadModel.Elastic;
using ReadModel.Models;
using ReadModel.Pg;

namespace ReadModel.Services;

public class EmployeeService
{
    private readonly PgContext _pgContext;
    private readonly ElasticRepository _elasticRepository;

    public EmployeeService(PgContext pgContext, ElasticRepository elasticRepository)
    {
        _pgContext = pgContext;
        _elasticRepository = elasticRepository;
    }

    public async Task<Employee> Create(Employee employee)
    {
        var result = await _pgContext.AddAsync(employee);
        await _pgContext.SaveChangesAsync();
        await _elasticRepository.IndexDocument(result.Entity);

        return result.Entity;
    }

    public async Task<Employee?> ById(int id)
    {
        return await _pgContext.Employees.FindAsync(id);
    }

    public async Task<Employee?> Update(Employee employee)
    {
        var result = _pgContext.Employees.Update(employee);
        await _pgContext.SaveChangesAsync();
        await _elasticRepository.UpdateDocument(employee);

        return result.Entity;
    }

    public async Task<Employee> Delete(int id)
    {
        var employee = await ById(id);
        if (employee is null)
        {
            throw new Exception("Not found");
        }

        var result = _pgContext.Remove(employee);
        await _pgContext.SaveChangesAsync();
        await _elasticRepository.DeleteDocument(result.Entity);

        return result.Entity;
    }
}