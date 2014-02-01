using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using ImoutoViewer.Commands;
using ImoutoViewer.Model;
using Microsoft.Win32;

namespace ImoutoViewer.ViewModel
{
    public class OpenWithVM
    {
        #region Constructors

        public OpenWithVM()
        {
            CurrentList = EditProgram.List;
        }

        #endregion //Constructors

        public List<EditProgram> CurrentList { get; set; }

    }

    public class EditProgram
    {
        #region Constructors

        private EditProgram()
        {
            ClickCommand = new RelayCommand(Click);
        }

        #endregion //Constructors

        #region Properties

        public string Name { get; set; }

        private string RegistryPath { get; set; }

        private string ExePath { get; set; }

        public string IconPath { get; set; }

        #endregion // Properties

        #region Commands

        public ICommand ClickCommand { get; set; }

        private void Click(object arg)
        {
            if (!(arg is string)) return;
            
            try
            {
                Process.Start(ExePath, arg as string);
            }
            catch { }
        }

        #endregion // Commands

        static EditProgram()
        {
            var list = new List<EditProgram>
            {
                new EditProgram()
                {
                    Name = "Photoshop", 
                    RegistryPath = @"Photoshop.exe\shell\edit\command",
                    IconPath = @"pack://application:,,,/Resources/img/edit/photoshop.ico"
                },
                new EditProgram()
                {
                    Name = "Paint", 
                    RegistryPath = @"mspaint.exe\shell\edit\command",
                    IconPath = @"pack://application:,,,/Resources/img/edit/ms_paint.ico"
                },
                new EditProgram()
                {
                    Name = "IrfanView", 
                    RegistryPath = @"i_view32.exe\shell\open\command",
                    IconPath = @"pack://application:,,,/Resources/img/edit/irfan_view.ico"
                },

            };

            foreach (var entry in list)
            {
                RegistryKey prog = Registry.ClassesRoot.OpenSubKey(@"Applications\" + entry.RegistryPath);
                if (prog != null)
                {
                    entry.ExePath =
                        prog.GetValue("")
                            .ToString()
                            .Split(new char[] {'\"'}, StringSplitOptions.RemoveEmptyEntries)
                            .First();
                }
            }
            List = list;
        }

        public static List<EditProgram> List;
    }
}