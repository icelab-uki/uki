﻿#pragma checksum "..\..\..\..\UKI\UKI_addIf_Basic.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E27BB5C828D462C34B662577AEF27EA6"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace P_Tracker2 {
    
    
    /// <summary>
    /// UKI_addIf_Basic
    /// </summary>
    public partial class UKI_addIf_Basic : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button butAdd;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox combo1;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox combo2;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboOpt;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/P_Tracker2;component/uki/uki_addif_basic.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
            ((P_Tracker2.UKI_addIf_Basic)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.butAdd = ((System.Windows.Controls.Button)(target));
            
            #line 6 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
            this.butAdd.Click += new System.Windows.RoutedEventHandler(this.butAdd_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.combo1 = ((System.Windows.Controls.ComboBox)(target));
            
            #line 7 "..\..\..\..\UKI\UKI_addIf_Basic.xaml"
            this.combo1.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.combo1_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.combo2 = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.comboOpt = ((System.Windows.Controls.ComboBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

