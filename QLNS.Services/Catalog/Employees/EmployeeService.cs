﻿using QLNS.DataAccess;
using QLNS.ViewModel.Catalogs.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QLNS.Entity.Entities;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QLNS.ViewModel.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLNS.ViewModel.Common;
using QLNS.Entity.Enums;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using QLNS.Services.Common;
using System.Net.Http.Headers;

namespace QLNS.Services.Catalog.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly QLNSDbContext _context;
        private readonly IStorageService _storageService;
        private const string USER_CONTENT_FOLDER_NAME = "user-content";

        public EmployeeService(QLNSDbContext context, IStorageService storageService)
        {
            _storageService = storageService;
            _context = context;
        }

        public async Task<int> Create(EmployeeCreateRequest request)
        {
            var Employee = new Entity.Entities.Employees()
            {
                ID = request.ID,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                DOB = request.DOB,
                Account = request.Account,
                Password = request.Password,
                PositionID = request.PositionID,
                RankID = request.RankID,
                CIC = request.CIC,
                NumberPhone = request.NumberPhone,
                Address = request.Address,
                Active = request.Active,
                Sex = request.Sex,
            };
            if (request.ThumbnailImage != null)
            {
                Employee.URLImage = await this.SaveFile(request.ThumbnailImage);
            }
            else Employee.URLImage = "";

            _context.Employee.Add(Employee);
            await _context.SaveChangesAsync();
            return Convert.ToInt32(Employee.ID);
        }

        public async Task<int> Delete(string EmployeeID)
        {
            var employee = await _context.Employee.FindAsync(EmployeeID);
            _context.Employee.Remove(employee);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<EmployeeVMStatistic>> GetemployeeVMStatistics()
        {
            var query = from p in _context.Employee select p;
            var data = await query.Select(x => new EmployeeVMStatistic()
            {
                ID = x.ID,
                Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
            }).ToListAsync();
            return data;
        }

        public async Task<IEnumerable<Entity.Entities.Employees>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<EmployeeViewModel>> GetAllPage(GetEmployeePagingRequest request)
        {
            var query = from p in _context.Employee
                        join pt in _context.Ranks on p.RankID equals pt.IDrank
                        join pp in _context.Positions on p.PositionID equals pp.IDposition
                        select new { p, pt, pp }
                        ;
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.p.ID.Contains(request.Keyword) || x.p.NumberPhone.Contains(request.Keyword));
            }
            int totalRow = await query.CountAsync();
            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new EmployeeViewModel()
                {
                    ID = x.p.ID,
                    FirstName = x.p.FirstName,
                    MiddleName = x.p.MiddleName,
                    LastName = x.p.LastName,
                    Sex = x.p.Sex,
                    NumberPhone = x.p.NumberPhone,
                    DOB = x.p.DOB,
                    CIC = x.p.CIC,
                    Address = x.p.Address,
                    Rank = x.pt.Name,
                    Position = x.pp.Name,
                    Account = x.p.Account,
                    Password = x.p.Password,
                    Active = x.p.Active,
                    URLImage = x.p.URLImage
                }).ToListAsync();
            var pagedView = new PagedResult<EmployeeViewModel>()
            {
                TotalRecords = totalRow,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = data
            };
            return pagedView;
        }

        public async Task<EmployeeViewModel> GetById(string EmployeeID)
        {
            var rank = (from p in _context.Employee
                        join pt in _context.Ranks on p.RankID equals pt.IDrank
                        where p.ID.Equals(EmployeeID)
                        select pt.Name).First();
            var position = (from p in _context.Employee
                            join pp in _context.Positions on p.PositionID equals pp.IDposition
                            where p.ID.Equals(EmployeeID)
                            select pp.Name).First();

            var employee = await _context.Employee.FindAsync(EmployeeID);
            var employeeviewmodel = new EmployeeViewModel()
            {
                ID = employee.ID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,
                DOB = employee.DOB,
                Account = employee.Account,
                Password = employee.Password,
                Position = position,
                Rank = rank,
                CIC = employee.CIC,
                NumberPhone = employee.NumberPhone,
                Address = employee.Address,
                Active = employee.Active,
                Sex = employee.Sex,
                URLImage = employee.URLImage,
            };
            return employeeviewmodel;
        }

        public async Task<EmployeeEditRequest> GetByIdEdit(string EmployeeID)
        {
            var x = await _context.Employee.FindAsync(EmployeeID);
            if (x == null) return null;
            var employeeviewmodel = new EmployeeEditRequest()
            {
                ID = x.ID,
                FirstName = x.FirstName,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                DOB = x.DOB,
                Sex = x.Sex,
                CIC = x.CIC,
                NumberPhone = x.NumberPhone,
                Address = x.Address,
                RankID = x.RankID,
                PositionID = x.PositionID,
                Account = x.Account,
                Active = x.Active,
                Password = x.Password,
                URLImage = x.URLImage
            };
            return employeeviewmodel;
        }

        public async Task<List<QLNS.Entity.Entities.Employees>> GetList()
        {
            var query = from p in _context.Employee select p;
            var data = await query.Select(x => new QLNS.Entity.Entities.Employees()
            {
                ID = x.ID,
                FirstName = x.FirstName,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                DOB = x.DOB,
                Sex = x.Sex,
                CIC = x.CIC,
                NumberPhone = x.NumberPhone,
                Address = x.Address,
                RankID = x.RankID,
                PositionID = x.PositionID,
                Account = x.Account,
                Password = x.Password
            }).ToListAsync();
            return data;
        }

        public async Task<int> Update(EmployeeEditRequest request)
        {
            var employee = await _context.Employee.FindAsync(request.ID);

            if (employee == null) return 0;
            employee.ID = request.ID;
            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.MiddleName = request.MiddleName;
            employee.DOB = request.DOB;
            employee.Sex = request.Sex;
            employee.CIC = request.CIC;
            employee.NumberPhone = request.NumberPhone;
            employee.Address = request.Address;
            employee.RankID = request.RankID;
            employee.PositionID = request.PositionID;
            employee.Account = request.Account;
            employee.Password = request.Password;
            employee.Active = request.Active;
            employee.URLImage = request.URLImage;
            _context.Employee.Update(employee);
            return await _context.SaveChangesAsync();
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return "/" + USER_CONTENT_FOLDER_NAME + "/" + fileName;
        }
    }
}