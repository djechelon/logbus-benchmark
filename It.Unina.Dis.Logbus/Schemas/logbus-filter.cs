﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:2.0.50727.4927
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.1432.
// 


/// <remarks/>
[System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(PropertyFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(FacilityEqualsFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(SeverityFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRegexMatchFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRegexNotMatchFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(FalseFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(TrueFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(NotFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(OrFilter))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(AndFilter))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("filter", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public abstract partial class FilterBase : object, System.ComponentModel.INotifyPropertyChanged {
    
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    
    protected void RaisePropertyChanged(string propertyName) {
        System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if ((propertyChanged != null)) {
            propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("parameter", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=true)]
public partial class FilterParameter : object, System.ComponentModel.INotifyPropertyChanged {
    
    private object valueField;
    
    private string nameField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public object value {
        get {
            return this.valueField;
        }
        set {
            this.valueField = value;
            this.RaisePropertyChanged("value");
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
            this.RaisePropertyChanged("name");
        }
    }
    
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    
    protected void RaisePropertyChanged(string propertyName) {
        System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if ((propertyChanged != null)) {
            propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("And", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class AndFilter : FilterBase {
    
    private FilterBase[] filterField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("filter")]
    public FilterBase[] filter {
        get {
            return this.filterField;
        }
        set {
            this.filterField = value;
            this.RaisePropertyChanged("filter");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("Or", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class OrFilter : FilterBase {
    
    private FilterBase[] filterField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("filter")]
    public FilterBase[] filter {
        get {
            return this.filterField;
        }
        set {
            this.filterField = value;
            this.RaisePropertyChanged("filter");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("Not", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class NotFilter : FilterBase {
    
    private FilterBase filterField;
    
    /// <remarks/>
    public FilterBase filter {
        get {
            return this.filterField;
        }
        set {
            this.filterField = value;
            this.RaisePropertyChanged("filter");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("True", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class TrueFilter : FilterBase {
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("False", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class FalseFilter : FilterBase {
}

/// <remarks/>
[System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRegexNotMatchFilter))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("MessageRegexMatch", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class MessageRegexMatchFilter : FilterBase {
    
    private string patternField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public string pattern {
        get {
            return this.patternField;
        }
        set {
            this.patternField = value;
            this.RaisePropertyChanged("pattern");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("MessageRegexNotMatch", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class MessageRegexNotMatchFilter : MessageRegexMatchFilter {
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("Severity", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class SeverityFilter : FilterBase {
    
    private ComparisonOperator comparisonField;
    
    private Severity severityField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public ComparisonOperator comparison {
        get {
            return this.comparisonField;
        }
        set {
            this.comparisonField = value;
            this.RaisePropertyChanged("comparison");
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public Severity severity {
        get {
            return this.severityField;
        }
        set {
            this.severityField = value;
            this.RaisePropertyChanged("severity");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
public enum ComparisonOperator {
    
    /// <remarks/>
    eq,
    
    /// <remarks/>
    neq,
    
    /// <remarks/>
    geq,
    
    /// <remarks/>
    gt,
    
    /// <remarks/>
    lt,
    
    /// <remarks/>
    leq,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
public enum Severity {
    
    /// <remarks/>
    Emergency,
    
    /// <remarks/>
    Alert,
    
    /// <remarks/>
    Critical,
    
    /// <remarks/>
    Error,
    
    /// <remarks/>
    Warning,
    
    /// <remarks/>
    Notice,
    
    /// <remarks/>
    Info,
    
    /// <remarks/>
    Debug,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("FacilityEquals", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class FacilityEqualsFilter : FilterBase {
    
    private FacilityEqualsFilterFacility facilityField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public FacilityEqualsFilterFacility facility {
        get {
            return this.facilityField;
        }
        set {
            this.facilityField = value;
            this.RaisePropertyChanged("facility");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
public enum FacilityEqualsFilterFacility {
    
    /// <remarks/>
    Kernel,
    
    /// <remarks/>
    User,
    
    /// <remarks/>
    Mail,
    
    /// <remarks/>
    System,
    
    /// <remarks/>
    Security,
    
    /// <remarks/>
    Internally,
    
    /// <remarks/>
    Printer,
    
    /// <remarks/>
    News,
    
    /// <remarks/>
    UUCP,
    
    /// <remarks/>
    Cron,
    
    /// <remarks/>
    Security2,
    
    /// <remarks/>
    FTP,
    
    /// <remarks/>
    NTP,
    
    /// <remarks/>
    Audit,
    
    /// <remarks/>
    Alert,
    
    /// <remarks/>
    Clock2,
    
    /// <remarks/>
    Local0,
    
    /// <remarks/>
    Local1,
    
    /// <remarks/>
    Local2,
    
    /// <remarks/>
    Local3,
    
    /// <remarks/>
    Local4,
    
    /// <remarks/>
    Local5,
    
    /// <remarks/>
    Local6,
    
    /// <remarks/>
    Local7,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("Property", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class PropertyFilter : FilterBase {
    
    private string valueField;
    
    private Property propertyNameField;
    
    private ComparisonOperator comparisonField;
    
    /// <remarks/>
    public string value {
        get {
            return this.valueField;
        }
        set {
            this.valueField = value;
            this.RaisePropertyChanged("value");
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public Property propertyName {
        get {
            return this.propertyNameField;
        }
        set {
            this.propertyNameField = value;
            this.RaisePropertyChanged("propertyName");
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
    public ComparisonOperator comparison {
        get {
            return this.comparisonField;
        }
        set {
            this.comparisonField = value;
            this.RaisePropertyChanged("comparison");
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
public enum Property {
    
    /// <remarks/>
    Timestamp,
    
    /// <remarks/>
    Severity,
    
    /// <remarks/>
    Facility,
    
    /// <remarks/>
    Host,
    
    /// <remarks/>
    ApplicationName,
    
    /// <remarks/>
    ProcessID,
    
    /// <remarks/>
    MessageID,
    
    /// <remarks/>
    Data,
    
    /// <remarks/>
    Text,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.dis.unina.it/logbus-ng/filters")]
[System.Xml.Serialization.XmlRootAttribute("Custom", Namespace="http://www.dis.unina.it/logbus-ng/filters", IsNullable=false)]
public partial class CustomFilter : FilterBase {
    
    private FilterParameter[] parameterField;
    
    private string nameField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("parameter", IsNullable=true)]
    public FilterParameter[] parameter {
        get {
            return this.parameterField;
        }
        set {
            this.parameterField = value;
            this.RaisePropertyChanged("parameter");
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
            this.RaisePropertyChanged("name");
        }
    }
}
