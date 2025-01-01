﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Domain.Common.Contracts
{
    // Apply this marker interface only to aggregate root entities
    // Repositories will only work with aggregate roots, not their children
    public interface IAggregateRoot : IEntity
    {
    }
}
