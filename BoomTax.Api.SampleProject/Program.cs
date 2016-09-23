using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Rest;
using BoomTax.Api.SampleProject.Models;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace BoomTax.Api.SampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Start().Wait();
        }

        private static async Task Start()
        {
            //string baseUrl = "https://api.boomtax.com";
            string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];

            HttpResponseMessage response = new HttpResponseMessage();

            using (var loginClient = new HttpClient { BaseAddress = new Uri(baseUrl) })
            {
                //login and get token
                var credentials = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "username", ConfigurationManager.AppSettings["UserName"] },
                    { "password", ConfigurationManager.AppSettings["Password"] },
                    { "grant_type", "password" }
                });

                response = await loginClient.PostAsync("Token", credentials);
            }

            if (response.IsSuccessStatusCode)
            {
                //parse token
                string loginResult = await response.Content.ReadAsStringAsync();
                dynamic data = JToken.Parse(loginResult);
                var token = data.access_token.ToString();

                using (BoomTaxApi client = new BoomTaxApi(new Uri(baseUrl), new TokenCredentials(token)))
                {
                    Console.WriteLine($"Getting filing types");
                    var filingTypes = await client.FilingType.GetFilingTypesAsync();

                    Console.WriteLine($"Getting filing types");

                    var aca2016FilingType = filingTypes.FirstOrDefault(o => o.TaxYear == 2016 && o.Name == "ACA Reporting (1094C/1095C)");

                    Console.WriteLine($"Creating filing...");
                    var filing = client.Filing.Post(aca2016FilingType.Id.Value, "ABC Corporation");
                    Console.WriteLine($"Filing id: {filing.Id}");

                    //all examples from pre-January 2016 test scenario
                    #region header form data        
                    var headerFormData = new Form1094C()
                    {
                        CompanyName = "Darrtestfive",
                        Ein = "000000599",
                        Address = "4689 Redwood Avenue",
                        City = "Austin",
                        State = "TX",
                        Zip = "78755",
                        CountryCode = "US",
                        ContactFirstName = "Susan",
                        ContactLastName = "Williamson",
                        ContactPhone = "5551234567",
                        TotalNumberOfForms = "322",
                        IsAuthoritativeTransmittal = true,
                        IsQualifyingOfferMethod = true,
                        IsSection4980HTransitionRelief = true,
                        IsMinimumOfferYesJan = true,
                        IsMinimumOfferYesFeb = true,
                        IsMinimumOfferYesMar = true,
                        IsMinimumOfferYesApr = true,
                        IsMinimumOfferYesMay = true,
                        IsMinimumOfferYesJun = true,
                        IsMinimumOfferYesJul = true,
                        IsMinimumOfferYesAug = true,
                        IsMinimumOfferYesSep = true,
                        IsMinimumOfferYesOct = true,
                        IsMinimumOfferYesNov = true,
                        IsMinimumOfferYesDec = true,
                        IsAggregatedGroupJan = true,
                        IsAggregatedGroupFeb = true,
                        IsAggregatedGroupMar = true,
                        IsAggregatedGroupApr = true,
                        IsAggregatedGroupMay = true,
                        IsAggregatedGroupJun = true,
                        IsAggregatedGroupJul = true,
                        IsAggregatedGroupAug = true,
                        IsAggregatedGroupSep = true,
                        IsAggregatedGroupOct = true,
                        IsAggregatedGroupNov = true,
                        IsAggregatedGroupDec = true,
                        IsSection4980hAll12MonthsSpecified = true,
                        Section4980HTransitionReliefIndicatorAll12Months = "A",
                        FullTimeEmployeeCountJan = 315,
                        TotalEmployeeCountJan = 330,
                        FullTimeEmployeeCountFeb = 316,
                        TotalEmployeeCountFeb = 335,
                        FullTimeEmployeeCountMar = 316,
                        TotalEmployeeCountMar = 335,
                        FullTimeEmployeeCountApr = 316,
                        TotalEmployeeCountApr = 335,
                        FullTimeEmployeeCountMay = 316,
                        TotalEmployeeCountMay = 335,
                        FullTimeEmployeeCountJun = 316,
                        TotalEmployeeCountJun = 335,
                        FullTimeEmployeeCountJul = 318,
                        TotalEmployeeCountJul = 335,
                        FullTimeEmployeeCountAug = 318,
                        TotalEmployeeCountAug = 333,
                        FullTimeEmployeeCountSep = 318,
                        TotalEmployeeCountSep = 333,
                        FullTimeEmployeeCountOct = 318,
                        TotalEmployeeCountOct = 333,
                        FullTimeEmployeeCountNov = 318,
                        TotalEmployeeCountNov = 333,
                        FullTimeEmployeeCountDec = 318,
                        TotalEmployeeCountDec = 333,
                        IsMemberOfAggregateAle = true,
                        Ein1 = "000000600",
                        Name1 = "Darrtestfive Subsidiary One"
                    };
                    #endregion

                    Console.WriteLine($"Adding header form...");
                    var headerFormId = client.Form1094C.Post(filing.Id.Value, headerFormData);
                    Console.WriteLine($"Header form id: {headerFormId}");

                    //populate our 1095-Cs
                    //this is using static data from test scenarios.
                    //you will likely want to pull this from your database
                    var form1095Cs = Get1095CData();

                    int lastFormId = 0;

                    //add 1095-Cs
                    foreach (var form in form1095Cs)
                    {
                        Console.WriteLine($"Adding form for employee: {form.FirstName} {form.LastName}");
                        lastFormId = client.Form1095C.Post(filing.Id.Value, form);
                        Console.WriteLine($"Created form with id: {lastFormId}");
                    }

                    //update a form (we changed the SSN in this case)
                    //make sure to include all fields, not just the updated fields.
                    #region updated form data
                    var updatedForm = new Form1095C
                    {
                        FirstName = "Odette",
                        MiddleName = "Cloudy",
                        LastName = "Davidson",
                        Address = "2993 Spruce Lane",
                        City = "Fort Collins",
                        State = "CO",
                        Zip = "80522",
                        CountryCode = "US",
                        EmployerPhone = "5551234545",
                        PlanStartMonth = "01",
                        Ssn = "000000534",
                        OfferOfCoverageAll12Months = "1A",
                        //EmployeeShareAll12Months = 0,
                        CoveredIndividuals = new List<CoveredIndividual>()
                        {
                            new CoveredIndividual()
                            {
                                FirstName = "Peter",
                                MiddleName = "",
                                LastName = "Davidson",
                                Dob = new DateTime(1970,2,6),
                                All12Months = true
                            },
                            new CoveredIndividual()
                            {
                                FirstName = "Mindy",
                                LastName = "Davidson",
                                Ssn = "000000534",
                                All12Months = true
                            },
                            new CoveredIndividual()
                            {
                                FirstName = "Nicolas",
                                MiddleName = "",
                                LastName = "Davidson",
                                Ssn = "000000535",
                                All12Months = true
                            },
                        }
                    };
                    #endregion

                    Console.WriteLine($"Updating form at id: {lastFormId}");
                    client.Form1095C.Put(lastFormId, updatedForm);

                    Console.WriteLine($"Deleting form at id: {lastFormId}");
                    client.Form1095C.Delete(lastFormId);
                    Console.WriteLine($"Form deleted.");

                    Console.WriteLine($"Initiating e-file request...");

                    //initiate e-file and add emails for notifications.
                    //note: This could generate more than 1 efile request depending on what has changed since the previous e-file.
                    IList<int> efileRequestIds = new List<int>();

                    //e-filing will not be available until late December/early January. 
                    //until then, this will throw an error.
                    try
                    {
                        efileRequestIds = client.EfileRequest.Post(filing.Id.Value, "hr@company.com", isSilent: false);
                    }
                    catch (HttpOperationException ex)
                    {
                        Console.WriteLine(ex.Response.AsFormattedString());
                    }


                    //*****************************************************************************************
                    //DO NOT POLL A GIVEN ID MORE THAN ONCE EVERY 15 MINUTES OR YOUR ACCOUNT MAY BE RESTRICTED.
                    //*****************************************************************************************
                    if (efileRequestIds.Any())
                    {
                        Console.WriteLine($"Checking e-file request...");
                        var efileRequest = client.EfileRequest.GetEfileRequest(efileRequestIds.First());

                        //if the e-file request is complete, this means we have a response from the IRS.
                        if (efileRequest.IsComplete ?? false)
                        {
                            Console.WriteLine($"E-File request complete. Getting response.");
                            var efileResponse = client.EfileResponse.Get(efileRequest.EfileResponseId.Value);

                            //get receipt and receipt detail
                            Console.WriteLine($"ReceiptId: {efileResponse.ReceiptId}, ReceivedOn: {efileResponse.ReceivedOn}, Status: {efileResponse.Status}");

                            //get form errors
                            foreach (var errorDetails in efileResponse.Errors)
                            {
                                Console.WriteLine($"FormId:{errorDetails.FormId}, ErrorCode:{errorDetails.ErrorCode}, ErrorMessage:{errorDetails.ErrorMessage}");
                            }
                        }
                    }


                    Console.WriteLine($"Deleting filing...");
                    client.Filing.Delete(filing.Id.Value);
                    Console.WriteLine($"Filing deleted.");
                }
            }
        }

        private static Form1095C[] Get1095CData()
        {
            var formRose = new Form1095C
            {
                FirstName = "Rose",
                MiddleName = "",
                LastName = "Davichi",
                Address = "847 Walnut Avenue",
                City = "Roy",
                State = "NM",
                Zip = "87743",
                CountryCode = "US",
                EmployerPhone = "5551234545",
                PlanStartMonth = "01",
                Ssn = "000000577",
                OfferOfCoverageAll12Months = "1E",
                EmployeeShareAll12Months = 0,
                CoveredIndividuals = new List<CoveredIndividual>()
                {
                    new CoveredIndividual()
                    {
                        FirstName = "Omar",
                        LastName = "Davichi",
                        Ssn="000000578",
                        All12Months = true
                    },
                    new CoveredIndividual()
                    {
                        FirstName = "Sam",
                        LastName = "Davichi",
                        Ssn = "000000579",
                        All12Months = true
                    },
                    new CoveredIndividual()
                    {
                        FirstName = "Erica",
                        LastName = "Davichi",
                        Dob = new DateTime(2005,12,5),
                        Jan = false,
                        Feb = false,
                        Mar = false,
                        Apr = false,
                        May = false,
                        Jun = false,
                        Jul = true,
                        Aug = true,
                        Sep = true,
                        Oct = true,
                        Nov = true,
                        Dec = true
                    }
                }
            };

            var formPeter = new Form1095C
            {
                FirstName = "Peter",
                MiddleName = "",
                LastName = "Davignon",
                Address = "5991 Sycamore Lane",
                City = "Sandy",
                State = "UT",
                Zip = "84094",
                CountryCode = "US",
                EmployerPhone = "5551234545",
                PlanStartMonth = "01",
                Ssn = "000000581",
                OfferOfCoverageAll12Months = "1A",
                //note, a difference between null and 0 here
                //EmployeeShareAll12Months = null,
                CoveredIndividuals = new List<CoveredIndividual>()
                {
                    new CoveredIndividual()
                    {
                        FirstName = "Sally",
                        MiddleName = "",
                        LastName = "Davignon",
                        Ssn="000000583",
                        All12Months = true
                    },
                    new CoveredIndividual()
                    {
                        FirstName = "Teddy",
                        MiddleName = "",
                        LastName = "Davignon",
                        Ssn = "000000589",
                        Dob = new DateTime(2015, 6, 28),
                        Jan = false,
                        Feb = false,
                        Mar = false,
                        Apr = false,
                        May = false,
                        Jun = true,
                        Jul = true,
                        Aug = true,
                        Sep = true,
                        Oct = true,
                        Nov = true,
                        Dec = true
                    }
                }
            };

            var formOdette = new Form1095C
            {
                FirstName = "Odette",
                MiddleName = "Cloudy",
                LastName = "Davidson",
                Address = "2993 Spruce Lane",
                City = "Fort Collins",
                State = "CO",
                Zip = "80522",
                CountryCode = "US",
                EmployerPhone = "5551234545",
                PlanStartMonth = "01",
                Ssn = "000000533",
                OfferOfCoverageAll12Months = "1A",
                //EmployeeShareAll12Months = 0,
                CoveredIndividuals = new List<CoveredIndividual>()
                {
                    new CoveredIndividual()
                    {
                        FirstName = "Peter",
                        MiddleName = "",
                        LastName = "Davidson",
                        Dob = new DateTime(1970,2,6),
                        All12Months = true
                    },
                    new CoveredIndividual()
                    {
                        FirstName = "Mindy",
                        LastName = "Davidson",
                        Ssn = "000000534",
                        All12Months = true
                    },
                    new CoveredIndividual()
                    {
                        FirstName = "Nicolas",
                        MiddleName = "",
                        LastName = "Davidson",
                        Ssn = "000000535",
                        All12Months = true
                    },
                }
            };

            return new[] { formRose, formPeter, formOdette };
        }
    }
}
