using FlightManagementProject.Facade;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TestForFlightManagmentProject;

namespace DBGenerator_FlightManagment_Part2
{
    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Random r = new Random();
        private APIReader apiReader = new APIReader();

        private int  howMuchAdmins;
        public int HowMuchAdmins { get { return this.howMuchAdmins; } set { this.howMuchAdmins = value; OnPropertyChanged("HowMuchAdmins"); } }

        private int howMuchCompanies;
        public  int HowMuchCompanies { get
            {
                if(this.howMuchCompanies < 1)
                {
                    if (this.howMuchFlights < 1)
                        return this.howMuchTickets;
                    return this.howMuchFlights;
                }
                return this.howMuchCompanies;
                    } set { this.howMuchCompanies = value; OnPropertyChanged("HowMuchCompanies"); MaxTickets = HowMuchFlights * HowMuchCompanies; } }

        private int  howMuchCustomers;
        public int HowMuchCustomers { get { return this.howMuchCustomers < 1 ? this.howMuchTickets : this.howMuchCustomers; } set { this.howMuchCustomers = value; OnPropertyChanged("HowMuchCustomers");} }

        private int  howMuchFlights;
        public int HowMuchFlights { get { return this.howMuchFlights < 1 ? this.howMuchTickets : this.howMuchFlights; } set { this.howMuchFlights = value; OnPropertyChanged("HowMuchFlights"); MaxTickets = HowMuchFlights * HowMuchCompanies; } }

        private int howMuchTickets;
        public int HowMuchTickets { get { return this.howMuchTickets; } set { this.howMuchTickets = value; OnPropertyChanged("HowMuchTickets"); } }
        public int MaxTickets { get
            {
                if (HowMuchCompanies * HowMuchFlights > 15)
                    return 15;
                return HowMuchCompanies * HowMuchFlights > 0 ? HowMuchCompanies * HowMuchFlights : 1;
            } set { OnPropertyChanged("MaxTickets"); } }

        private int  howMuchCountries;
        public int HowMuchCountries { get { return this.howMuchCountries; } set { this.howMuchCountries = value; OnPropertyChanged("HowMuchCountries"); } }

        private static int totalOfInstances;
        public static int TotalOfInstances{ get { return totalOfInstances; } set { totalOfInstances = value; StaticOnPropertyChanged("TotalOfInstances"); }}

        private static double howMuchCreated;
        public static double HowMuchCreated { get { return howMuchCreated; } set { howMuchCreated = value; StaticOnPropertyChanged("HowMuchCreated"); } }

        private string totalAndCreated;
        public string TotalAndCreated { get { return totalAndCreated; } set { totalAndCreated = $"{TotalOfInstances}:{HowMuchCreated}"; OnPropertyChanged("TotalAndCreated"); } }

        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand ReplaceCommand { get; set; }
        public bool readerIsFree = true;

        private static ObservableCollection<string> logger;
        public static ObservableCollection<string> Logger { get { return logger; }
            set
            {
                logger = value;
            }
        }

        public ViewModel()
        {

            log4net.Config.XmlConfigurator.Configure();
            Logger = new ObservableCollection<string>();
            AddCommand = new DelegateCommand(() =>
            {
                AddToDB();
            }, () =>
            {
                return readerIsFree;
            }
            );

            ReplaceCommand = new DelegateCommand(() =>
            {
                Task.Run(() =>
                {
                    DeleteAllDB();
                    Thread.Sleep(500);
                    AddToDB();
                });
            }, () =>
            {
                return readerIsFree;
            }
            );

            Task.Run(() =>
            {
                while (true)
                {
                    AddCommand.RaiseCanExecuteChanged();
                    ReplaceCommand.RaiseCanExecuteChanged();
                   Thread.Sleep(500);
                }
            });

            HowMuchCountries = 1;
            TotalOfInstances = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void StaticOnPropertyChanged(string property)
        {
            if (StaticPropertyChanged != null)
            {
                StaticPropertyChanged(null, new PropertyChangedEventArgs(property));
            }
        }


        private void DeleteAllDB()
        {
            readerIsFree = false;
                try
                {
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"Start Deletion All The Database."))));
                    using (SqlConnection conn = new SqlConnection(FlyingCenterConfig.CONNECTION_STRING))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("Delete From Tickets;" +
                            "Delete From Flights;" +
                            "Delete From AirlineCompanies;" +
                            "Delete From Customers;" +
                            "Delete From Countries;" +
                            "Delete From Administrators;" +
                            "Delete From Users", conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.RecordsAffected > 0)
                                {
                                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"Delete Database Successfully Completed."))));
                                log.Info("All The Database Has Been Deleted.");
                            }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                log.Info($"The System Encountered An Error When Try To Delete The Database: {ex.StackTrace}\n {ex.Message}.");
                MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"Error: {ex.StackTrace}\n {ex.Message}\n Try Again.."))));
                    HowMuchCreated = 0;
                    TotalOfInstances = 0;
                }
        }
        private void AddToDB()
        {
            readerIsFree = false;
            howMuchCreated = 0;
            TotalOfInstances = HowMuchAdmins + HowMuchCompanies + HowMuchCustomers + (HowMuchFlights * HowMuchCompanies) + (HowMuchTickets * HowMuchCustomers) + HowMuchCountries;
            log.Info($"********** Start Generation Action: **********");

            Task.Run(() => 
            {
                try
                {
                    apiReader.ReadAdminsFromAPI(HowMuchAdmins);
                    apiReader.ReadCountriesFromAPI(HowMuchCountries);
                    apiReader.ReadCompaniesFromAPI(HowMuchCompanies);
                    apiReader.ReadCustomersFromAPI(HowMuchCustomers);
                    apiReader.ReadFlightsFromAPI(HowMuchFlights);
                    apiReader.ReadTicketsFromAPI(HowMuchTickets);
                }
                catch (Exception ex)
                {
                    log.Info($"The System Encountered An Error When Try To Add Item To Database: {ex.StackTrace}\n {ex.Message}.");
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"Error: {ex.StackTrace}\n {ex.Message}\n Try Again.."))));
                    HowMuchCreated = 0;
                    TotalOfInstances = 0;
                }
                finally
                {
                    readerIsFree = true;

                    // Generation Is Done.
                    Thread.Sleep(500);
                    log.Info($"\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\nResult Of Current Generation: {ViewModel.TotalOfInstances} Items Added To Your Database.\n{ViewModel.TotalOfInstances - ViewModel.HowMuchCreated} Failed.\n\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\n");
                    MainWindow.m_Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new ThreadStart(new Action(() => ViewModel.Logger.Add($"\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\nGeneration Is  Done!\n\nResult: {ViewModel.TotalOfInstances} Items Added. ({ViewModel.TotalOfInstances - ViewModel.HowMuchCreated} Failed)\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"))));

                }
            });
        }
    }
}