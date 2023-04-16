using System;
using System.Collections.Generic;

namespace SimpleDI;

internal delegate void ServiceInjectionSetter(object instance, object service);

internal record DelegateObjectInjectionInfo(ICollection<SetterDelegateInfo> Setters, Type InstanceType);

internal record SetterDelegateInfo(ServiceInjectionSetter Delegate, Type ServiceType);