﻿#pragma checksum "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7923572CE4ED9AFAE2DBFFC804F846B3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using P_Tracker2;
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
    /// TexturedFaceMeshViewer
    /// </summary>
    public partial class TexturedFaceMeshViewer : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ColorImage;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewport3D viewport3d;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.MeshGeometry3D theGeometry;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DiffuseMaterial theMaterial;
        
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
            System.Uri resourceLocater = new System.Uri("/P_Tracker2;component/tracker/texturedfacemeshviewer.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Tracker\TexturedFaceMeshViewer.xaml"
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
            this.ColorImage = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.viewport3d = ((System.Windows.Controls.Viewport3D)(target));
            return;
            case 3:
            this.theGeometry = ((System.Windows.Media.Media3D.MeshGeometry3D)(target));
            return;
            case 4:
            this.theMaterial = ((System.Windows.Media.Media3D.DiffuseMaterial)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

