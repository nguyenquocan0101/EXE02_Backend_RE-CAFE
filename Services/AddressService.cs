using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(Guid userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.ReceiverName)
                .ToListAsync();

            return addresses.Select(MapToDto);
        }

        public async Task<AddressDto?> GetMyAddressByIdAsync(Guid userId, Guid addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            return address == null ? null : MapToDto(address);
        }

        public async Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressRequest request)
        {
            var hasAnyAddress = await _context.Addresses.AnyAsync(a => a.UserId == userId);
            var shouldSetDefault = request.IsDefault || !hasAnyAddress;

            if (shouldSetDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
            }

            var address = new Address
            {
                UserId = userId,
                ReceiverName = request.ReceiverName.Trim(),
                Phone = request.Phone.Trim(),
                Province = request.Province.Trim(),
                District = request.District.Trim(),
                Ward = request.Ward.Trim(),
                DetailAddress = request.DetailAddress.Trim(),
                IsDefault = shouldSetDefault
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return MapToDto(address);
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            if (request.IsDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
                address.IsDefault = true;
            }
            else if (address.IsDefault)
            {
                // Keep one default address at all times.
                address.IsDefault = true;
            }
            else
            {
                address.IsDefault = false;
            }

            address.ReceiverName = request.ReceiverName.Trim();
            address.Phone = request.Phone.Trim();
            address.Province = request.Province.Trim();
            address.District = request.District.Trim();
            address.Ward = request.Ward.Trim();
            address.DetailAddress = request.DetailAddress.Trim();

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return MapToDto(address);
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            var wasDefault = address.IsDefault;
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            if (wasDefault)
            {
                var nextAddress = await _context.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.ReceiverName)
                    .FirstOrDefaultAsync();

                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    _context.Addresses.Update(nextAddress);
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            await UnsetDefaultAddressesAsync(userId);
            address.IsDefault = true;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return MapToDto(address);
        }

        private async Task UnsetDefaultAddressesAsync(Guid userId)
        {
            var defaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            if (!defaultAddresses.Any())
            {
                return;
            }

            foreach (var address in defaultAddresses)
            {
                address.IsDefault = false;
            }

            _context.Addresses.UpdateRange(defaultAddresses);
            await _context.SaveChangesAsync();
        }

        private static AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                ReceiverName = address.ReceiverName,
                Phone = address.Phone,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                DetailAddress = address.DetailAddress,
                IsDefault = address.IsDefault
            };
        }
    }
}
