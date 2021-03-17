using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class MyWordsBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }

        [Inject]
        DefaultLangsService DefaultLangsService { get; set; }
        [Inject]
        public CurrentUser CurrentUser { get; set; }

        [Parameter]
        public string UserId { get; set; }

        [Parameter]
        public string UserName { get; set; }
        protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 0;
        protected string TLang;


        protected List<WordDto> Words { get; set; }

        protected void NewWordHandler(WordDto wordDto)
        {
            //var newWord = new WordModel
            //{
            //    WordId = wordDto.WordId,
            //    WordLang = wordDto.WordLang,
            //    ToLang = wordDto.ToLang,
            //    UserId = wordDto.UserId,
            //    Explain = wordDto.Explain,
            //    ShareWith = wordDto.ShareWith,
            //    Title = wordDto.Title
            //};

            Words.Add(wordDto);
        }
        protected void DeleteWordHandler(WordDto wordDto)
        {
            //var newWord = new WordModel
            //{
            //    WordId = wordDto.WordId,
            //    WordLang = wordDto.WordLang,
            //    ToLang = wordDto.ToLang,
            //    UserId = wordDto.UserId,
            //    Explain = wordDto.Explain,
            //    ShareWith = wordDto.ShareWith,
            //    Title = wordDto.Title
            //};
            Words.Remove(wordDto);
        }

        protected async Task InitAsync()
        {
            //if (string.IsNullOrWhiteSpace(currUserId))
            //{
            //    currUserId = CurrUsrService.id;
            //}
            //else
            //    NavigationManager.NavigateTo("/");

            TLang = DefaultLangsService.DefaultToLang;

            //try
            //{
            await GetNextPage();
            //}
            //catch
            //{
            //    NavigationManager.NavigateTo("/");
            //}
        }

        protected async Task GetNextPage()
        {
            loading = true;
            currentPage++;

            if (Words == null)
            {
                Words = new List<WordDto>();
            }
            var res = await WordService.GetPageWordsFromUserID(CurrentUser.id, UserId, 10, currentPage);
            if (res != null)
            {
                currentPage = res.Item1;
                foreach (var w in res.Item2)
                {
                    //var wordDto = new WordDto
                    //{
                    //    WordId = w.WordId,
                    //    WordLang = w.WordLang,
                    //    ToLang = w.ToLang,
                    //    UserId = w.UserId,
                    //    Title = w.Title,
                    //    Explain = w.Explain,
                    //    CommentsCont = w.CommentsCont,
                    //    IsILiked = w.IsILiked,
                    //    LikesCount = w.LikesCount,
                    //    ShareWith = w.ShareWith
                    //};
                    Words.Add(w);
                }
            }
            loading = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            Words = null;
            await InitAsync();
        }
    }
}
