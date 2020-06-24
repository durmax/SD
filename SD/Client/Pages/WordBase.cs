using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class WordBase : ComponentBase
    {
        //[CascadingParameter]
        //private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public bool Collapsed { set; get; } = true;    // hide by default

        protected bool opCollapsed { set; get; } = true;    // hide by default

        [Inject]
        public WordService WordService { set; get; }

        [Parameter]
        public WordModel wordModel { get; set; }

        [Parameter]
        public string UserId { get; set; }

        protected string styleDeleted;
        protected string cssClassDelete = "d-none";
        protected string cssClassUpdate = "d-none";
        protected bool loading;
        protected string note;

        protected int Rows = 2;

        string _myText;
        protected string MyText
        {
            get => _myText;
            set
            {
                _myText = value;
                CalculateSize(value);
            }
        }

        private void CalculateSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Rows = Math.Max(value.Split('\n').Length, value.Split('\r').Length);
                Rows = Math.Max(Rows, 2);
                Rows = Math.Min(Rows, 20);
            }
        }

        protected async Task DeleteWord()
        {
            var response = await WordService.RemoveWord(wordModel.WordId);
            if (response.IsSuccessStatusCode)
            {
                cssClassDelete = "d-none";
                styleDeleted = "text-decoration: line-through;";
                note = "Deleted";
            }
            //x = await response.Content.ReadAsStringAsync();

        }
        protected void Reverse()
        {
            string l = wordModel.WordLang;
            wordModel.WordLang = wordModel.ToLang;
            wordModel.ToLang = l;
        }

        protected async Task AddWord()
        {
            note = "...";
            if (string.IsNullOrWhiteSpace(wordModel.UserId) || UserId != wordModel.UserId)
            {
                wordModel.WordId = Guid.NewGuid().ToString();
                wordModel.UserId = UserId;
                wordModel.CreatedAt = DateTime.Now;
            }
            wordModel.Explain = MyText;
            HttpResponseMessage respons = await WordService.AddWord(wordModel);

            if (!respons.IsSuccessStatusCode)
            {
                if ((int)respons.StatusCode == 302)
                {
                    cssClassUpdate = "";
                    wordModel.WordId = await respons.Content.ReadAsStringAsync();
                }
                //note = await respons.Content.ReadAsStringAsync();
            }
            else
            {
                note = "Saved";
                if ((int)respons.StatusCode == 200)
                {
                    NewWord();
                    MyText = "";
                }
            }
        }
        protected async Task UpdateWord()
        {
            note = "...";
            cssClassUpdate = "d-none";
            HttpResponseMessage respons = await WordService.UpdateWord(wordModel);
            if (!respons.IsSuccessStatusCode)
            {
                //note = $"Sorry, {wordModel.Title} did not updated!";
                note = await respons.Content.ReadAsStringAsync();
            }
            else
            {
                note = "Updated";
                NewWord();
                MyText = "";
            }
        }

        private void NewWord()
        {
            string wl = wordModel.WordLang;
            string tl = wordModel.ToLang;
            wordModel = new WordModel
            {
                WordId = Guid.NewGuid().ToString(),
                WordLang = wl,
                ToLang = tl,
                UserId = UserId,
                CreatedAt = DateTime.Now
            };
        }

        protected void WordChanged(string title)
        {
            opCollapsed = false;
            note = "";
            wordModel.Title = title;

        }
        protected override void OnInitialized()
        {
            MyText = wordModel.Explain ?? "";

            if (!string.IsNullOrWhiteSpace(wordModel.UserId) && wordModel.UserId != UserId)
            {
                cssClassDelete = "d-none";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(wordModel.WordId))
                {
                    cssClassDelete = "";
                }
            }
        }
    }
}
