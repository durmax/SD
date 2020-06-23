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
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public bool Collapsed { set; get; } = true;    // hide by default

        protected bool opCollapsed { set; get; } = true;    // hide by default

        [Inject]
        public WordService WordService { set; get; }

        [Parameter]
        public WordModel wordModel { get; set; }
        protected string UserId { get; set; }

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
            if (!string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(wordModel.Title))
            {
                WordModel foundWord = await SearchByTitle(wordModel.Title);

                if (foundWord.WordId != null)
                {
                    wordModel = foundWord;
                    // MyText += "\n " + (wordModel.Explain ?? MyText);
                    wordModel.Explain = MyText;

                    //note = "You have saved this word before, it is updated.";
                    cssClassUpdate = "";
                }

                else
                {
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
                        note = $"Sorry, {wordModel.Title} did not saved!";
                    }
                    else
                    {
                        wordModel.Title = "";
                        MyText = "";
                        wordModel.WordId = Guid.NewGuid().ToString();
                    }
                }
            }
        }
        protected async Task UpdateWord()
        {
            cssClassUpdate = "d-none";
            HttpResponseMessage respons = await WordService.UpdateWord(wordModel);
            if (!respons.IsSuccessStatusCode)
            {
                note = $"Sorry, {wordModel.Title} did not updated!";
            } 
            else
            {
                wordModel.Title = "";
                MyText = "";
                wordModel.WordId = Guid.NewGuid().ToString();
            }
        }

        protected async Task<WordModel> SearchByTitle(string title)
        {
            WordModel foundWord = new WordModel();
            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(UserId))
            {
                foundWord = await WordService.GetWordByText(UserId, title);
            }
            return foundWord;
        }
        protected void WordChanged(string title)
        {
            opCollapsed = false;
            note = "";
            wordModel.Title = title;

        }
        protected override async Task OnInitializedAsync()
        {
            MyText = wordModel.Explain ?? "";
            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                UserId = user.FindFirst(c => c.Type == "oid")?.Value;

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
}
