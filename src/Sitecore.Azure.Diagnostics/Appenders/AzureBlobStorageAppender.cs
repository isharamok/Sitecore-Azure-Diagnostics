using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Appender;
using log4net.spi;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;

namespace Sitecore.Azure.Diagnostics.Appenders
{
  /// <summary>
  /// Represents the appender that logs Sitecore diagnostic information to Microsoft Azure Blob storage service.
  /// </summary>
  public class AzureBlobStorageAppender : AppenderSkeleton
  {
    #region Fields

    /// <summary>
    /// Gets or sets the BLOB URI.
    /// </summary>
    /// <value>
    /// The BLOB URI.
    /// </value>
    public string BlobUri { get; set; }

    /// <summary>
    /// Gets or sets the current date.
    /// </summary>
    /// <value>
    /// The current date.
    /// </value>
    public DateTime CurrentDate { get; protected set; }

    /// <summary>
    /// The cloud BLOB for storing log entries.
    /// </summary>
    private ICloudBlob cloudBlob;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the BLOB.
    /// </summary>
    /// <value>
    /// The cloud BLOB.
    /// </value>
    private ICloudBlob Blob
    {
      get
      {
        // Create a new blob if this is the first time it is used.
        if (this.cloudBlob == null)
        {
          this.cloudBlob = this.GetNewBlob();
        }
        // Recreate a blob if a container is no longer exists.
        else if (!this.cloudBlob.Container.Exists())
        {
          this.cloudBlob = LogStorageManager.GetBlob(this.cloudBlob.Name);
        }
        // Create a new blob if the current shouldn't be used.
        else
        {
          DateTime now = DateTime.Now;
          bool needNewBlob = (this.CurrentDate.Day != now.Day || this.CurrentDate.Month != now.Month || this.CurrentDate.Year != now.Year);

          if (needNewBlob)
          {
            this.cloudBlob = this.GetNewBlob();
          }
        }

        return this.cloudBlob;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageAppender"/> class.
    /// </summary>
    public AzureBlobStorageAppender()
    {
      this.BlobUri = "log.{date}.txt";
      this.CurrentDate = DateTime.Now; 
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Appends the specified logging event.
    /// </summary>
    /// <param name="loggingEvent">The logging event.</param>
    protected override void Append(LoggingEvent loggingEvent)
    {
      Sitecore.Diagnostics.Assert.ArgumentNotNull(loggingEvent, "loggingEvent");

      //var blob = this.Blob as CloudBlockBlob;
      string message = this.RenderLoggingEvent(loggingEvent);

      //this.AddMessageToBlock(blob, message);
      this.AddMessageToBlob(this.Blob, message);
    }
    protected virtual void AddMessageToBlob(ICloudBlob blob, string message)
    {
      if (blob is CloudBlockBlob)
      {
        var blockBlob = blob as CloudBlockBlob;
        AddMessageToBlock(blockBlob, message);
      }
      else if (blob is CloudAppendBlob)
      {
        var appendBlob = blob as CloudAppendBlob;
        AddMessageToAppend(appendBlob, message);
      }
      else if (blob is CloudPageBlob)
      {
        var pageBlob = blob as CloudPageBlob;
      }
    }

    /// <summary>
    /// Adds the diagnostic message to block blob.
    /// </summary>
    /// <param name="blob">The cloud blob.</param>
    /// <param name="message">The message.</param>
    protected virtual void AddMessageToBlock(CloudBlockBlob blob, string message)
    {
      Sitecore.Diagnostics.Assert.ArgumentNotNull(blob, "blob");
      Sitecore.Diagnostics.Assert.ArgumentNotNull(message, "message");

      var blockIds = new List<string>();

      if (blob.Exists())
      {
        blockIds.AddRange(blob.DownloadBlockList().Select(b => b.Name));
      }

      string blockId = Guid.NewGuid().ToString().Replace("-", string.Empty);
      blockIds.Add(blockId);

      using (var blockData = new MemoryStream(LogStorageManager.DefaultTextEncoding.GetBytes(message), false))
      {
        blob.PutBlock(blockId, blockData, null);
        blob.PutBlockList(blockIds);
      }
    }
    /// <summary>
    /// Adds the diagnostic message to append blob.
    /// </summary>
    /// <param name="blob">The cloud append blob.</param>
    /// <param name="message">The message.</param>
    protected virtual void AddMessageToAppend(CloudAppendBlob blob, string message)
    {
      Sitecore.Diagnostics.Assert.ArgumentNotNull(blob, "blob");
      Sitecore.Diagnostics.Assert.ArgumentNotNull(message, "message");
      using (var blockData = new MemoryStream(LogStorageManager.DefaultTextEncoding.GetBytes(message), false))
      {
        blob.AppendBlock(blockData);
      }
    }
    /// <summary>
    /// Adds the diagnostic message to page blob.
    /// </summary>
    /// <param name="blob">The cloud page blob.</param>
    /// <param name="message">The message.</param>
    protected virtual void AddMessageToPage(CloudPageBlob blob, string message)
    {
      Sitecore.Diagnostics.Assert.ArgumentNotNull(blob, "blob");
      Sitecore.Diagnostics.Assert.ArgumentNotNull(message, "message");
      throw new NotImplementedException("AddMessageToPage is not implemented.");
    }

    /// <summary>
    /// Gets the new cloud blob for diagnostic messages.
    /// </summary>
    /// <returns>
    /// The cloud blob.
    /// </returns>
    protected virtual ICloudBlob GetNewBlob()
    {
      var blob = LogStorageManager.CreateBlob(this.ConstructBlobNameByDate());

      if (blob.Exists())
      {
        blob = LogStorageManager.CreateBlob(this.ConstructBlobNameByDateTime());
      }

      return blob;
    }

    /// <summary>
    /// Construct the blob name using the current date.
    /// </summary>
    /// <returns>
    /// The blob name based on the current date.
    /// </returns>
    protected virtual string ConstructBlobNameByDate()
    {
      return this.BlobUri.Replace("{date}", this.CurrentDate.ToString("yyyyMMdd"));
    }

    /// <summary>
    /// Constructs the blob name using the current date and time.
    /// </summary>
    /// <returns>
    /// The BLOB name based on the current date and time.
    /// </returns>
    protected virtual string ConstructBlobNameByDateTime()
    {
      string name = this.ConstructBlobNameByDate();
      int dotIndex = name.LastIndexOf('.');

      if (dotIndex < 0)
      {
        return name;
      }

      return name.Substring(0, dotIndex) + '.' + this.CurrentDate.ToString("HHmmss") + name.Substring(dotIndex);
    }
    
    #endregion
  }
}