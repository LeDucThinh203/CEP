using System.Security.Claims;
using CEP.Backend.Common;
using CEP.Backend.DTOs.Customers;
using CEP.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEP.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    private string? GetCurrentUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerQueryDto queryDto)
    {
        var result = await _customerService.GetCustomersAsync(queryDto);
        return Ok(ApiResponse<PagedResult<CustomerDto>>.SuccessResult(result, "Lấy danh sách khách hàng thành công."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse.Fail($"Không tìm thấy khách hàng với ID = {id}."));
        }

        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Fail("Dữ liệu không hợp lệ.", errors));
        }

        try
        {
            var created = await _customerService.CreateAsync(dto, GetCurrentUsername());
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<CustomerDto>.SuccessResult(created, "Thêm khách hàng thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Fail("Dữ liệu không hợp lệ.", errors));
        }

        var updated = await _customerService.UpdateAsync(id, dto, GetCurrentUsername());
        if (!updated)
        {
            return NotFound(ApiResponse.Fail($"Không tìm thấy khách hàng với ID = {id} để cập nhật."));
        }

        return Ok(ApiResponse.Ok("Cập nhật khách hàng thành công."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id, GetCurrentUsername());
        if (!deleted)
        {
            return NotFound(ApiResponse.Fail($"Không tìm thấy khách hàng với ID = {id} để xóa."));
        }

        return Ok(ApiResponse.Ok("Xóa khách hàng thành công."));
    }
}
