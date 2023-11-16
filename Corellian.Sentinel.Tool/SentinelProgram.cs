using System.Collections.Immutable;
using System.CommandLine;
using System.Data;
using System.Diagnostics;
using Corellian.Sentinel.Tool.Configuration;
using Terminal.Gui;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Corellian.Sentinel.Tool
{
    public class SentinelProgram
    {
        public static async Task<int> Main(string[] args)
        {
            var fileOption = new Option<FileInfo>(
                aliases: new [] { "--config", "-c" },
                description: "The config file.")
                {
                    IsRequired = true
                };

            var rootCommand = new RootCommand("Corellian Sentinel")
            {
                Name = "sentinel"
            };
            rootCommand.AddOption(fileOption);

            rootCommand.SetHandler(Run, fileOption);

            return await rootCommand.InvokeAsync(args);
        }

        private static void Run(FileInfo file)
        {
            var configString = File.ReadAllText(file.FullName);
            var mergingParser = new MergingParser(new Parser(new StringReader(configString)));
            var deserializer = new DeserializerBuilder()
                .Build();
            var serializer = new SerializerBuilder()
                .Build();

            var configuration = deserializer.Deserialize<SentinelConfiguration.Builder>(mergingParser).Build();

            var applications = configuration.Applications.Select(c => new SentinelApplication(c.Key, c.Value)).ToImmutableList();

            Application.Init();

            // Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black);

            var window = new Window("Applications")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var tableView = new TableView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = new DataTable()
            };

            tableView.Table.Columns.Add(new DataColumn("Name", typeof(string)));
            tableView.Table.Columns.Add(new DataColumn("Status", typeof(string)));
            tableView.Table.Columns.Add(new DataColumn("AutoRestart", typeof(bool)));
            tableView.Table.Columns.Add(new DataColumn("PID", typeof(double)));
            tableView.Table.Columns.Add(new DataColumn("CPU", typeof(double)));
            tableView.Table.Columns.Add(new DataColumn("Memory", typeof(double)));
            tableView.Table.Columns.Add(new DataColumn("Uptime", typeof(TimeSpan)));


            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["Name"], new TableView.ColumnStyle
            {
                MinWidth = 15
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["Status"], new TableView.ColumnStyle
            {
                MinWidth = 9
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["AutoRestart"], new TableView.ColumnStyle
            {
                MinWidth = 8
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["PID"], new TableView.ColumnStyle
            {
                MinWidth = 7
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["CPU"], new TableView.ColumnStyle
            {
                Format = "0.00%",
                MinWidth = 6
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["Memory"], new TableView.ColumnStyle
            {
                Format = "0",
                MinWidth = 6
            });

            tableView.Style.ColumnStyles.Add(tableView.Table.Columns["Uptime"], new TableView.ColumnStyle
            {
            });

            tableView.FullRowSelect = true;
            tableView.Style.ShowVerticalHeaderLines = false;
            tableView.Style.ShowVerticalCellLines = false;
            tableView.Style.ShowHorizontalScrollIndicators = false;

            foreach (var application in applications)
            {
                tableView.Table.Rows.Add(
                    application.Name,
                    application.Status.ToString(),
                    application.ProcessId,
                    application.CpuUsage,
                    application.MemoryUsage,
                    application.Uptime);
            }

            window.Add(tableView);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_Sentinel", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => {
                        Application.RequestStop();
                    })
                }),
            });

            var statusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.CtrlMask | Key.S, "~^S~ Stop/Start", () =>
                {
                    var selectedName = tableView.Table.Rows[tableView.SelectedRow][tableView.Table.Columns["Name"]].ToString();

                    var application = applications.Single(a => a.Name == selectedName);

                    if (application.Status == ApplicationStatus.Unknown || application.Status == ApplicationStatus.Stopped)
                    {
                        application.Start();
                    }
                    else if (application.Status == ApplicationStatus.Running)
                    {
                        application.Stop();
                    }
                }),
                new StatusItem(Key.CtrlMask | Key.R, "~^R~ Restart", () => {}),
                new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () =>
                {
                    Application.RequestStop();
                })
            });



            Application.Top.Add(menu, window, statusBar);

            void MonitorApplications()
            {
                foreach (var application in applications)
                {
                    if (application.Status == ApplicationStatus.Unknown || application.Status == ApplicationStatus.Stopped)
                    {
                        if (application.AutoRestart)
                        {
                            application.TryRestart();
                        }
                    }
                    else
                    {
                        application.Monitor();
                    }
                }
            }

            void UpdateTable()
            {
                for (int i = 0; i < applications.Count; i++)
                {
                    tableView.Table.Rows[i][tableView.Table.Columns["Status"]] = applications[i].Status;
                    tableView.Table.Rows[i][tableView.Table.Columns["AutoRestart"]] = applications[i].AutoRestart;
                    tableView.Table.Rows[i][tableView.Table.Columns["PID"]] = applications[i].ProcessId == null ? DBNull.Value : applications[i].ProcessId;
                    tableView.Table.Rows[i][tableView.Table.Columns["CPU"]] = applications[i].CpuUsage == null ? DBNull.Value : applications[i].CpuUsage;
                    tableView.Table.Rows[i][tableView.Table.Columns["Memory"]] = applications[i].MemoryUsage == null ? DBNull.Value : applications[i].MemoryUsage;
                    tableView.Table.Rows[i][tableView.Table.Columns["Uptime"]] = applications[i].Uptime == null ? DBNull.Value : applications[i].Uptime;
                }

                tableView.Update();
            }

            _ = Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1), m =>
            {
                MonitorApplications();
                UpdateTable();
                return true;
            });

            Application.Run();

            Debug.WriteLine("Shutdown 1");
            Application.Shutdown();
            Debug.WriteLine("Shutdown 2");
        }
    }
}