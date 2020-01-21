using Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Admin.Controllers
{
    [Route("api/domains")]
    [ApiController]
    [Authorize]
    public class DomainsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DomainsController(IAuthorizationService authorizationService, UserRepository userRepository,
            MainDbContext context)
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _context = context;
        }

        // GET: api/domains/mine
        [Authorize(Roles = Roles.AdminUser)]
        [HttpGet("mine")]
        public ActionResult<IEnumerable<Domain>> GetDomains([FromQuery] GetDomainsParam param)
        {
            var user = (AdminUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.DomainId);
            var query = _context.Domain.Where(a => a.OrganizationCode == domain.OrganizationCode);

            // order by
            if (param.OrderBy == OrderKey.Asc) query = OrderBy(query, param.SortBy.ToString());
            if (param.OrderBy == OrderKey.Desc) query = OrderByDescending(query, param.SortBy.ToString());

            // paging
            return query.Skip((param.Page - 1) * param.PageSize).Take(param.PageSize).ToList();
        }

        // GET: api/domains?organizationCode=5
        [Authorize(Roles = Roles.SuperAdminUser)]
        [HttpGet]
        public ActionResult<IEnumerable<Domain>> GetDomains([FromQuery] GetDomainsAdminParam param)
        {
            var query = _context.Domain.Where(a => a.OrganizationCode == param.OrganizationCode);

            // order by
            if (param.OrderBy == OrderKey.Asc) query = OrderBy(query, param.SortBy.ToString());
            if (param.OrderBy == OrderKey.Desc) query = OrderByDescending(query, param.SortBy.ToString());

            // paging
            return query.Skip((param.Page - 1) * param.PageSize).Take(param.PageSize).ToList();
        }




        // TODO: move
        // see Sorting using property name as string, https://entityframeworkcore.com/knowledge-base/34899933/
        private static IOrderedQueryable<TSource> OrderBy<TSource>(IQueryable<TSource> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");
            Expression property = Expression.Property(parameter, propertyName);
            return (IOrderedQueryable<TSource>)typeof(Queryable).GetMethods()
                .First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), property.Type).Invoke(null,
                    new object[] { source, Expression.Lambda(property, parameter) });
        }

        private static IOrderedQueryable<TSource> OrderByDescending<TSource>(IQueryable<TSource> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");
            Expression property = Expression.Property(parameter, propertyName);
            return (IOrderedQueryable<TSource>)typeof(Queryable).GetMethods()
                .First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), property.Type).Invoke(null,
                    new object[] { source, Expression.Lambda(property, parameter) });
        }

        public enum SortKey
        {
            Id,
            DomainName,
        }

        public enum OrderKey
        {
            Asc,
            Desc,
        }

        public class GetDomainsParam
        {
            [Range(1, int.MaxValue)]
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;

            public SortKey SortBy { get; set; } = SortKey.DomainName;
            public OrderKey OrderBy { get; set; } = OrderKey.Asc;
        }

        public class GetDomainsAdminParam : GetDomainsParam
        {
            public long? OrganizationCode { get; set; }
        }
    }
}
