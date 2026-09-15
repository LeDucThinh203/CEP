using System.Text.Json;
using CEP.Backend.Common;
using CEP.Backend.Data;
using CEP.Backend.DTOs.Customers;
using CEP.Backend.Interfaces;
using CEP.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CEP.Backend.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public CustomerService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerQueryDto queryDto)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        // 1. Search by FullName or PhoneNumber at Database level
        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var search = queryDto.Search.Trim();
            query = query.Where(x => x.FullName.Contains(search) || x.PhoneNumber.Contains(search) || x.CustomerCode.Contains(search));
        }

        // 2. Filter by Active / Inactive
        if (queryDto.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == queryDto.IsActive.Value);
        }

        // 3. Count total matching items before pagination
        var totalItems = await query.CountAsync();

        // 4. Sorting
        query = (queryDto.SortBy?.ToLower()) switch
        {
            "customercode" => queryDto.SortDescending ? query.OrderByDescending(x => x.CustomerCode) : query.OrderBy(x => x.CustomerCode),
            "fullname" => queryDto.SortDescending ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "createdat" => queryDto.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.Id)
        };

        // 5. Pagination: Ensure OrderBy precedes Skip and Take
        var page = queryDto.Page > 0 ? queryDto.Page : 1;
        var pageSize = queryDto.PageSize > 0 ? queryDto.PageSize : 10;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                DateOfBirth = x.DateOfBirth,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<CustomerDto>(items, totalItems, page, pageSize);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
        {
            return null;
        }

        return new CustomerDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DateOfBirth = customer.DateOfBirth,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, string? currentUsername = null)
    {
        // Check uniqueness of CustomerCode
        var exists = await _context.Customers
            .AnyAsync(x => x.CustomerCode.ToLower() == dto.CustomerCode.Trim().ToLower());

        if (exists)
        {
            throw new InvalidOperationException($"Mã khách hàng '{dto.CustomerCode}' đã tồn tại.");
        }

        var customer = new Customer
        {
            CustomerCode = dto.CustomerCode.Trim(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            DateOfBirth = dto.DateOfBirth,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Write Audit Log
        var newValues = JsonSerializer.Serialize(new
        {
            customer.CustomerCode,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.IsActive
        });

        await _auditService.LogAsync(
            currentUsername ?? "System",
            "CREATE",
            nameof(Customer),
            customer.Id,
            null,
            newValues);

        return new CustomerDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DateOfBirth = customer.DateOfBirth,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto, string? currentUsername = null)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
        if (customer == null)
        {
            return false;
        }

        var oldValues = JsonSerializer.Serialize(new
        {
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.IsActive
        });

        // Update allowable properties (CustomerCode cannot be modified)
        customer.FullName = dto.FullName.Trim();
        customer.Email = dto.Email.Trim();
        customer.PhoneNumber = dto.PhoneNumber.Trim();
        customer.DateOfBirth = dto.DateOfBirth;
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var newValues = JsonSerializer.Serialize(new
        {
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.IsActive
        });

        await _auditService.LogAsync(
            currentUsername ?? "System",
            "UPDATE",
            nameof(Customer),
            customer.Id,
            oldValues,
            newValues);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string? currentUsername = null)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
        if (customer == null)
        {
            return false;
        }

        var oldValues = JsonSerializer.Serialize(new
        {
            customer.CustomerCode,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.IsActive
        });

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            currentUsername ?? "System",
            "DELETE",
            nameof(Customer),
            id,
            oldValues,
            null);

        return true;
    }
}
