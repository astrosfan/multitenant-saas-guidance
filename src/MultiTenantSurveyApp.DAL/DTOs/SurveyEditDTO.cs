﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantSurveyApp.DAL.DTOs
{
    public class SurveyEditDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        public IEnumerable<QuestionDTO> Questions { get; set; }
    }
}
