﻿namespace Vivi.Infrastructure.Entities
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}