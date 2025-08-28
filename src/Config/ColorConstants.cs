using System;
using System.Windows.Media;

namespace AccountManager.Config
{
    public static class ColorConstants
    {
        // Dark Theme Colors
        public static class Dark
        {
            // Main backgrounds
            public const string Background = "#1e1e31";      // Main app background
            public const string Surface = "#1E293B";         // Cards, sidebar
            public const string Border = "#334155";          // Borders, dividers
            
            // Text colors
            public const string TextPrimary = "#F1F5F9";     // Main text
            public const string TextSecondary = "#94A3B8";   // Secondary text
            public const string TextAccent = "#bdbdff";      // Accent text
            
            // Input specific colors
            public const string InputBackground = "#191926";
            public const string InputBorder = "#38384c";
            public const string InputBorderFocus = "#c5c4fe";
            public const string InputPlaceholder = "#bab4d3";
            public const string InputLabelBackground = "#6366F1";
            public const string InputLabelText = "#FFFFFF";
            public const string InputText = "#FFFFFF";
            
            // Sidebar specific
            public const string SidebarBackground = "#181826";
            public const string AppBarBackgroundColor = "#1e1e31";
            public const string SidebarTextPrimary = "#FFFFFF";
            public const string SidebarTextSecondary = "#94A3B8";
            
            // Special backgrounds
            public const string SearchBackground = "#334155";
            public const string EmptyStateBackground = "#334155";
            public const string CardBackground = "#26263b";
            public const string CardBorderHover = "#6366F1";
            
            // UI State colors
            public const string HoverBackground = "#23232e";
            public const string SelectedBackground = "#302d45";
            public const string GlowColor = "#c5c4fe";
            
            // Context Menu colors
            public const string ContextMenuBackground = "#2a2839";
            public const string ContextMenuBorder = "#3d3a4b";
        }

        // Light Theme Colors
        public static class Light
        {
            // Main backgrounds
            public const string Background = "#F1F5F9";      // Main app background
            public const string Surface = "#FFFFFF";         // Cards, sidebar
            public const string Border = "#E2E8F0";          // Borders, dividers
            
            // Text colors
            public const string TextPrimary = "#1E293B";     // Main text
            public const string TextSecondary = "#64748B";   // Secondary text
            public const string TextAccent = "#4f14bd";      // Accent text

            // Input specific colors
            public const string InputBackground = "#F8FAFC";
            public const string InputBorder = "#E2E8F0";
            public const string InputBorderFocus = "#6366F1";
            public const string InputPlaceholder = "#94A3B8";
            public const string InputLabelBackground = "#6366F1";
            public const string InputLabelText = "#FFFFFF";
            public const string InputText = "#1E293B";
            
            // Sidebar specific
            public const string SidebarBackground = "#FFFFFF";
            public const string AppBarBackgroundColor = "#181826";
            public const string SidebarTextPrimary = "#1E293B";
            public const string SidebarTextSecondary = "#64748B";
            
            // Special backgrounds
            public const string SearchBackground = "#F8FAFC";
            public const string EmptyStateBackground = "#F8FAFC";
            public const string CardBackground = "#ffffffff";
            public const string CardBorderHover = "#6366F1";
            
            // UI State colors
            public const string HoverBackground = "#F1F5F9";
            public const string SelectedBackground = "#E2E8F0";
            public const string GlowColor = "#6366F1";
            
            // Context Menu colors
            public const string ContextMenuBackground = "#FFFFFF";
            public const string ContextMenuBorder = "#E2E8F0";
        }

        // Common Colors (theme-independent)
        public static class Common
        {
            public const string Primary = "#6366F1";         // Primary brand color
            public const string Accent = "#8B5CF6";          // Secondary brand color
            public const string Success = "#10B981";         // Success/positive actions
            public const string Danger = "#EF4444";          // Errors/negative actions
            public const string Warning = "#F59E0B";         // Warnings/alerts
        }

        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentException("Hex color cannot be null or empty", nameof(hex));
            
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static SolidColorBrush ToBrush(string hex)
        {
            return new SolidColorBrush(FromHex(hex));
        }

        public static SolidColorBrush ToBrush(string hex, double opacity)
        {
            var brush = ToBrush(hex);
            brush.Opacity = opacity;
            return brush;
        }
    }
}