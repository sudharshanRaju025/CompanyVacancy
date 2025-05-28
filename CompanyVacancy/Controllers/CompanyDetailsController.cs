using CompanyVacancy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;


namespace CompanyVacancy.Controllers
{
    public class CompanyDetailsController : Controller
    {
        private readonly AddDbFile _dbContext;

        public CompanyDetailsController(AddDbFile dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(CompanyDetails viewModel)
        {
            
            var results = await _dbContext.CompanyDetails.ToListAsync();
            return View(results);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 4)
        {
            IQueryable<CompanyDetails> companies = _dbContext.CompanyDetails;
            string fetch = SearchQueryParser.ParseToDynamicLinq(query);

            if (!string.IsNullOrWhiteSpace(query))
            {
                
                companies = companies.Where(fetch);
            }

            int totalItems = await companies.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var results = await companies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Query = query; // So pagination keeps search text

            return View(results);
        }


        //public async Task<IActionResult> Search(string query)
        //{
        //    IQueryable<CompanyDetails> fetch = _dbContext.CompanyDetails;
        //    string retrive = SearchQueryParser.ParseToDynamicLinq(query);
        //    var results = await _dbContext.CompanyDetails
        //        .Where(r => r.Name.Contains(retrive))
        //        .ToListAsync();
        //    return View(results);
        //}
        //public class CompanyDetailsViewModel
        //{
        //    public List<CompanyDetails> Companies { get; set; } = new();
        //    public int CurrentPage { get; set; }
        //    public int TotalPages { get; set; }
        //    public string Query { get; set; } = string.Empty;
        //}

        //IQueryable<CompanyDetails> Fetch = _dbContext.CompanyDetails;
        //var predicate = Parse.Converting(query);
        //if (!string.IsNullOrWhiteSpace(predicate))
        //{
        //    var result = _dbContext.CompanyDetails
        //        .Where(predicate).Order();

        //var fetching = result.ToList();
        //    return View(fetching);
        //}

        //var fetch = Fetch.Where(s => s.Name == query || s.Location == query).ToList();

        //return View(fetch);

        //public IQueryable<CompanyDetails> SearchCompanies(DbContext db, string userInput)
        //{
        //    var whereClause = SearchQueryParser.ParseToDynamicLinq(userInput);
        //    return db.Set<CompanyDetails>().Where(whereClause);
        //}

    }






}

