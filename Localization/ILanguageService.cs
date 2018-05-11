using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Localization
{
    public static class SupportedLanguages
    {
        public struct LangeuageInfo
        {
            public string Code { get; }
            public string NativeName { get; }

            public LangeuageInfo(string nativeName, string code)
            {
                Code = code;
                NativeName = nativeName;
            }
        }

        public static ImmutableArray<LangeuageInfo> Languages =
            new[]
            {
                new LangeuageInfo("English",   "en"), // Default
                new LangeuageInfo("Deutsch",   "de"),
            }.ToImmutableArray();

        public static LangeuageInfo Default { get; } = new LangeuageInfo("English", "en");
    }

    /// <summary>
    /// Event data for language change events.
    /// </summary>
    public class LanguageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Use this property to set Thread.CurrentCulture;
        /// </summary>
        public CultureInfo Culture;
        /// <summary>
        /// Use this property to set Thread.CurrentUICulture;
        /// </summary>
        public CultureInfo UiCulture;
    }
    /// <summary>
    /// Service interface for managing language changes runtime.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Use this property to set Thread.CurrentCulture;
        /// </summary>
        CultureInfo Culture { get; set; }
        /// <summary>
        /// Use this property to set Thread.CurrentUICulture;
        /// </summary>
        CultureInfo UiCulture { get; set; }
        /// <summary>
        /// Event fired when culture and ui culture changes.
        /// </summary>
        event EventHandler<LanguageChangedEventArgs> CultureChanged;
        /// <summary>
        /// Set current thread cultures to <see cref="Culture"/> and  <see cref="UiCulture"/>.
        /// </summary>
        void UpdateCurrentThreadCultures();
    }
    /// <summary>
    /// Abstract implementation of ILanguageService.
    /// </summary>
    public class DefaultLanguageService : ILanguageService
    {
        public DefaultLanguageService(CultureInfo culture, CultureInfo uiCulture)
        {
            Culture = culture;
            UiCulture = uiCulture;
        }

        private CultureInfo _culture;
        public CultureInfo Culture
        {
            get { return _culture; }
            set
            {
                if (!Equals(_culture, value))
                {
                    _culture = value;
                    CultureInfo.DefaultThreadCurrentCulture = value;
                    UpdateCurrentThreadCultures();
                    RaiseCultureChangedEvent();
                }
            }
        }
        private CultureInfo _uiCulture;
        public CultureInfo UiCulture
        {
            get { return _uiCulture; }
            set
            {
                if (!Equals(_uiCulture, value))
                {
                    _uiCulture = value;
                    CultureInfo.DefaultThreadCurrentUICulture = value;
                    UpdateCurrentThreadCultures();
                    RaiseCultureChangedEvent();
                }
            }
        }
        public event EventHandler<LanguageChangedEventArgs> CultureChanged;
        public void UpdateCurrentThreadCultures()
        {
            if (Culture != null)
                Thread.CurrentThread.CurrentCulture = Culture;
            if (UiCulture != null)
                Thread.CurrentThread.CurrentUICulture = UiCulture;
        }
        private void RaiseCultureChangedEvent()
        {
            CultureChanged?.Invoke(this, new LanguageChangedEventArgs()
            {
                Culture = _culture,
                UiCulture = _uiCulture,
            });
        }
    }
}
