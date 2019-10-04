using FlightManagementProject;
using FlightManagementProject.DAO;
using FlightManagementProject.Facade;
using FlightManagementProject.Poco_And_User;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TestForFlightManagmentProject;

namespace DBGenerator_FlightManagment_Part2
{
    public class APIReader
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Random r = new Random();
        private LoggedInAdministratorFacade adminFacade = new LoggedInAdministratorFacade();
        private LoginToken<Administrator> adminToken = new LoginToken<Administrator> { User = new Administrator(FlyingCenterConfig.ADMIN_NAME, FlyingCenterConfig.ADMIN_PASSWORD) };
        List<string> userNamesOfCustomers = new List<string>();
        List<string> userNamesOfCompanies = new List<string>();
        private const string usersURL = "https://randomuser.me/api";
        private HttpClient client = new HttpClient();

        public APIReader()
        {
            client.BaseAddress = new Uri(usersURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            log4net.Config.XmlConfigurator.Configure();
        }

        // 1. Read Random Admin From API Web.
        public void ReadAdminsFromAPI(int times)
        {
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Adminsitrators..."))));
                int success = 0;
                for (int i = 0; i < times; i++)
                {
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        APIUser adminAPI = response.Content.ReadAsAsync<APIUser>().Result;
                        adminFacade.CreateNewAdmin(adminToken, new Administrator(adminAPI.results[0].login.username, adminAPI.results[0].login.password));
                        MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {i + 1}/{times} Administrators Was Generated.")));
                        success++;
                    }
                    ViewModel.HowMuchCreated++;
                }
            if (times > 0)
            {
                log.Info($"\n{success} Administrators Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Administrators Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Administrators.")));
        }

        // 2. Read Random Country From API Web (Foriegn Key For Companies).
        public void ReadCountriesFromAPI(int times)
        {
            int success = 0;
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Countries..."))));
            for (int i = 0; i < times; i++)
            {
                HttpResponseMessage response =  client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    APIUser countryAPI = response.Content.ReadAsAsync<APIUser>().Result;
                    adminFacade.CreateNewCountry(adminToken, new Country(countryAPI.results[0].name.first));
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {i + 1}/{times} Countries Was Generated.")));
                    success++;
                }
                ViewModel.HowMuchCreated++;
            }
            if (times > 0)
            {
                log.Info($"\n{success} Countries Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Countries Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Countries.")));
        }

        // 3. Read Random Customer From API Web.
        public void ReadCustomersFromAPI(int times)
        {
            int success = 0;
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Customers..."))));
            for (int i = 0; i < times; i++)
            {
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    APIUser customerAPI = response.Content.ReadAsAsync<APIUser>().Result;
                    Customer customer = new Customer(customerAPI.results[0].name.first, customerAPI.results[0].name.last, customerAPI.results[0].login.username, customerAPI.results[0].login.password,
                        customerAPI.results[0].location.city, customerAPI.results[0].phone, customerAPI.results[0].cell);
                    customer.Id = adminFacade.CreateNewCustomer(adminToken, customer);
                    userNamesOfCustomers.Add(customer.User_Name);
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {i + 1}/{times} Customers Was Generated.")));
                    success++;
                }
                    ViewModel.HowMuchCreated++;
            }
            if (times > 0)
            {
                log.Info($"\n{success} Customers Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Customers Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Customers.")));
        }

        // 4. Read Random Company From API Web.
        public void ReadCompaniesFromAPI(int times)
        {
            int success = 0;
            IList<Country> countries = new AnonymousUserFacade().GetAllCountries();
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Companies..."))));
            for (int i = 0; i < times; i++)
            {
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    APIUser companyAPI = response.Content.ReadAsAsync<APIUser>().Result;
                    AirlineCompany airline = new AirlineCompany(companyAPI.results[0].name.first, companyAPI.results[0].login.username, companyAPI.results[0].login.password, 
                        (int)countries[r.Next(countries.Count)].Id);
                    airline.Id = adminFacade.CreateNewAirline(adminToken, airline);
                    userNamesOfCompanies.Add(airline.User_Name);
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {i + 1}/{times} Companies Was Generated.")));
                    success++;
                }
                    ViewModel.HowMuchCreated++;
            }
            if (times > 0)
            {
                log.Info($"\n{success} Companies Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Companies Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Companies.")));

        }

        // 5. Read Random Flight From API Web.
        public void ReadFlightsFromAPI(int times)
        {
            int success = 0;
            IList<AirlineCompany> companies = new AnonymousUserFacade().GetAllAirlineCompanies();
            IList<Country> countries = new AnonymousUserFacade().GetAllCountries();
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Flights..."))));
            for (int i = 0; i < userNamesOfCompanies.Count; i++)
            {
                FlyingCenterSystem.GetUserAndFacade(adminFacade.GetAirlineByUserName(adminToken, userNamesOfCompanies[i]).User_Name,
                     adminFacade.GetAirlineByUserName(adminToken, userNamesOfCompanies[i]).Password, out ILogin token, out FacadeBase facade);
                LoginToken<AirlineCompany> airlineToken = token as LoginToken<AirlineCompany>;
                LoggedInAirlineFacade airlineFacade = facade as LoggedInAirlineFacade;
                for (int j = 0; j < times; j++)
                {
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        APIUser flightAPI = response.Content.ReadAsAsync<APIUser>().Result;
                        airlineFacade.CreateFlight(airlineToken, new Flight(airlineToken.User.Id, countries[r.Next(countries.Count)].Id, 
                            countries[r.Next(countries.Count)].Id, flightAPI.results[0].registered.date, flightAPI.results[0].registered.date + TimeSpan.FromHours(r.Next(1, 12)), r.Next(1000)));
                        MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {success+1}/{times*userNamesOfCompanies.Count} Flights Was Generated.")));
                        success++;
                    }
                    ViewModel.HowMuchCreated++;
                }
            }
            userNamesOfCompanies = new List<string>();
            if (times > 0)
            {
                log.Info($"\n{success} Flights Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Flights Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Flights.")));
        }
        
        // 6. Create Random Ticket From API Web.
        public void ReadTicketsFromAPI(int times)
        {
            int success = 0;
            IList<Customer> customers = adminFacade.GetAllCustomers(adminToken);
            IList<Flight> flights = new AnonymousUserFacade().GetAllFlights();
            MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add("Start Create Tickets..."))));
            for (int i = 0; i < userNamesOfCustomers.Count; i++)
            {
                FlyingCenterSystem.GetUserAndFacade(adminFacade.GetCustomerByUserName(adminToken, userNamesOfCustomers[i]).User_Name, adminFacade.GetCustomerByUserName(adminToken, userNamesOfCustomers[i]).Password, out ILogin token, out FacadeBase facade);
                LoginToken<Customer> customerToken = token as LoginToken<Customer>;
                LoggedInCustomerFacade customerFacade = facade as LoggedInCustomerFacade;
                for (int j = 0; j < times; j++)
                {
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        APIUser ticketAPI = response.Content.ReadAsAsync<APIUser>().Result;
                        customerFacade.PurchaseTicket(customerToken , flights[j]);
                        MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- {success+1}/{times * userNamesOfCustomers.Count} Tickets Was Generated.")));
                        success++;
                    }
                        ViewModel.HowMuchCreated++;
                    
                }
            }
            userNamesOfCustomers = new List<string>();
            if (times > 0)
            {
                log.Info($"\n{success} Tickets Were Created And {times - success} Failed.\n");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"- Tickets Generator Is Over. ({success} Were Created And {times - success} Failed)."))));
            }
            else
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger[ViewModel.Logger.Count - 1] = $"- No Creation Request For Tickets.")));
        }

    }
    
    public class Name
    {
        public string first;
        public string last;
    }
    
    public class Login
    {
        public string username;
        public string password;
    }
    
    public class Location
    {
        public string city;
    }

    public class Registered
    {
        public DateTime date;
    }

    public class APIUser
    {
        public ProjectUser[] results;
    }

    public class ProjectUser
    {
        public Name name;
        public Login login;
        public Location location;
        public string phone;
        public string cell;
        public Registered registered;
    }
}