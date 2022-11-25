using Microsoft.AspNetCore.Mvc;
using Pay1193.Services;
using Pay1193.Entity;
using Pay1193.Models;
using System.Dynamic;

namespace Pay1193.Controllers
{
    public class PaymentRecordController : Controller
    {
        private readonly IPaymentRecord _paymentRecordService;
        private readonly IEmployee _employeeService;
        private readonly ITaxService _taxService;
        private readonly INationalInsuranceService _nationalInsuranceService;
        private readonly IPayService _payService;

        public PaymentRecordController(IPaymentRecord paymentRecordService, IEmployee employeeService, ITaxService taxService, INationalInsuranceService nationalInsuranceService, IPayService payService)
        {
            _paymentRecordService = paymentRecordService;
            _employeeService = employeeService;
            _taxService = taxService;
            _nationalInsuranceService = nationalInsuranceService;
            _payService = payService;
        }

        public IActionResult Index()
        {
            var paymentRecords = _paymentRecordService.GetAll().Select(paymentRecordItem => new PaymentRecordIndexViewModel
            {
                FullName = _employeeService.GetById(paymentRecordItem.EmployeeId).FullName,
                DatePay = paymentRecordItem.DatePay,
                MonthPay = paymentRecordItem.MonthPay,
                TaxYearId = paymentRecordItem.TaxYearId,
                TaxYear = _taxService.GetById(paymentRecordItem.TaxYearId).YearOfTax,
                TotalEarnings = paymentRecordItem.TotalEarnings,
                EarningDeduction = paymentRecordItem.EarningDeduction,
                NetPayment = paymentRecordItem.NetPayment,
            }).ToList();

            return View(paymentRecords);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.employees = _employeeService.GetAll();
            ViewBag.taxYears = _paymentRecordService.GetAllTaxYear();
            var model = new PaymentRecordCreateViewModel();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PaymentRecordCreateViewModel model)
        {
            var paymentRecord = new PaymentRecord
            {
                EmployeeId = model.EmployeeId,
                Employee = _employeeService.GetById(model.EmployeeId),
                DatePay = model.DatePay,
                MonthPay = model.MonthPay,
                TaxYearId = model.TaxYearId,
                TaxYear = _taxService.GetById(model.TaxYearId),
                HourlyRate = model.HourlyRate,
                HourWorked = model.HourWorked,
                ContractualHours = model.ContractualHours,
                ContractualEarnings = _payService.ContractualEarning(model.ContractualHours, model.HourWorked, model.HourlyRate),

                TaxCode = ""
            };


            await _paymentRecordService.CreateAsync(paymentRecord);
            return RedirectToAction("Index");
        }  
    }
}
