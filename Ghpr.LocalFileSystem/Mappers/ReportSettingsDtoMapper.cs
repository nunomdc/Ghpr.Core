﻿using Ghpr.Core.Common;
using Ghpr.LocalFileSystem.Entities;

namespace Ghpr.LocalFileSystem.Mappers
{
    public static class ReportSettingsDtoMapper
    {
        public static ReportSettings Map(this ReportSettingsDto rsDto)
        {
            var rs = new ReportSettings(rsDto.RunsToDisplay, rsDto.TestsToDisplay, rsDto.ReportName);
            return rs;
        }

        public static ReportSettingsDto ToDto(this ReportSettings rs)
        {
            var rsDto = new ReportSettingsDto(rs.TestsToDisplay, rs.RunsToDisplay, rs.ReportName);
            return rsDto;
        }
    }
}