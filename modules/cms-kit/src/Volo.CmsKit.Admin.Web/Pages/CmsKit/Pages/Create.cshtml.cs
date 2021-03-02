﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using Volo.Abp.Validation;
using Volo.CmsKit.Admin.Pages;
using Volo.CmsKit.Admin.Web.Pages;
using Volo.CmsKit.Pages;

namespace Volo.CmsKit.Admin.Web.Pages.CmsKit.Pages
{
    public class CreateModel : CmsKitAdminPageModel
    {
        protected readonly IPageAdminAppService pageAdminAppService;

        [BindProperty]
        public CreatePageViewModel ViewModel { get; set; }

        public CreateModel(IPageAdminAppService pageAdminAppService)
        {
            this.pageAdminAppService = pageAdminAppService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var createInput = ObjectMapper.Map<CreatePageViewModel, CreatePageInputDto>(ViewModel);

            var created = await pageAdminAppService.CreateAsync(createInput);

            return new OkObjectResult(created);
        }

        [AutoMap(typeof(CreatePageInputDto), ReverseMap = true)]
        public class CreatePageViewModel
        {
            [DynamicMaxLength(typeof(PageConsts), nameof(PageConsts.MaxTitleLength))]
            [Required]
            public string Title { get; set; }

            [DynamicMaxLength(typeof(PageConsts), nameof(PageConsts.MaxSlugLength))]
            [Required]
            [DynamicFormIgnore]
            public string Slug { get; set; }
        }
    }
}
