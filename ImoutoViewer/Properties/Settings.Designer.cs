﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Imouto.Viewer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.0.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DownscaleToViewPort")]
        public global::Imouto.ResizeType ResizeType {
            get {
                return ((global::Imouto.ResizeType)(this["ResizeType"]));
            }
            set {
                this["ResizeType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Folder, Subfolders")]
        public global::Imouto.Viewer.Model.DirectorySearchFlags DirectorySearchFlags {
            get {
                return ((global::Imouto.Viewer.Model.DirectorySearchFlags)(this["DirectorySearchFlags"]));
            }
            set {
                this["DirectorySearchFlags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ByName")]
        public global::Imouto.Viewer.Model.SortMethod FilesSorting {
            get {
                return ((global::Imouto.Viewer.Model.SortMethod)(this["FilesSorting"]));
            }
            set {
                this["FilesSorting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ByName")]
        public global::Imouto.Viewer.Model.SortMethod FoldersSorting {
            get {
                return ((global::Imouto.Viewer.Model.SortMethod)(this["FoldersSorting"]));
            }
            set {
                this["FoldersSorting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FilesSortingDesc {
            get {
                return ((bool)(this["FilesSortingDesc"]));
            }
            set {
                this["FilesSortingDesc"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FoldersSortingDesc {
            get {
                return ((bool)(this["FoldersSortingDesc"]));
            }
            set {
                this["FoldersSortingDesc"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Blue")]
        public string AccentColorName {
            get {
                return ((string)(this["AccentColorName"]));
            }
            set {
                this["AccentColorName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ThemeIndex {
            get {
                return ((int)(this["ThemeIndex"]));
            }
            set {
                this["ThemeIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int SlideshowDelay {
            get {
                return ((int)(this["SlideshowDelay"]));
            }
            set {
                this["SlideshowDelay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowTags {
            get {
                return ((bool)(this["ShowTags"]));
            }
            set {
                this["ShowTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowNotes {
            get {
                return ((bool)(this["ShowNotes"]));
            }
            set {
                this["ShowNotes"] = value;
            }
        }
    }
}
