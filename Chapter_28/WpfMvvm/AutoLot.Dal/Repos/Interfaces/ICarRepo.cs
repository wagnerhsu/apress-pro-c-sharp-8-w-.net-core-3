﻿using System.Collections.Generic;
using AutoLot.Dal.Models.Entities;
using AutoLot.Dal.Repos.Base;

namespace AutoLot.Dal.Repos.Interfaces
{
    public interface ICarRepo : IRepo<Car>
    {
        IEnumerable<Car> GetAllBy(int makeId);
    }
}
