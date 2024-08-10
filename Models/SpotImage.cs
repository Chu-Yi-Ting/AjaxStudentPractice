using System;
using System.Collections.Generic;

namespace AjaxDemo.Models;

public partial class SpotImage
{
    public int ImageId { get; set; }

    public int? SpotId { get; set; }

    public string? ImageTitle { get; set; }

    public string? ImagePath { get; set; }
}
