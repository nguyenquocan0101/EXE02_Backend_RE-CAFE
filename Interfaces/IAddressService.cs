using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetMyAddressesAsync(Guid userId);
        Task<AddressDto?> GetMyAddressByIdAsync(Guid userId, Guid addressId);
        Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressRequest request);
        Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request);
        Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
        Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId);
    }
}
