using System;
using System.Collections.Generic;

namespace SimpleDI.Internal;
internal record struct DelayedTargetInfo(HashSet<IDelayedDependencyInjection> Targets, Dictionary<Type, bool> Dependencies);
