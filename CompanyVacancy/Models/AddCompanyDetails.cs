using CompanyVacancy.Models;

namespace CompanyVacancy
{
    public class AddCompanyDetails
    {
        
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Vacancies { get; set; } 
        public string Job_roles { get; set; } = null!;
        public int Need_Factor { get; set; } 
        public string Vacancy_Factor { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Contact { get; set; } = null!;
    }
}