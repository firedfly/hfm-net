﻿using AutoMapper;

using HFM.Core.Services;

namespace HFM.Forms.Models;

public class UserStatsDataModelProfile : Profile
{
    public UserStatsDataModelProfile()
    {
        CreateMap<UserStatsData, UserStatsDataModel>()
            .DisableCtorValidation()
            .ForMember(dest => dest.ControlsVisible, opt => opt.Ignore());
    }
}
