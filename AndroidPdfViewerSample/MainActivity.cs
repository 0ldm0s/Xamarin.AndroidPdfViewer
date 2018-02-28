using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Net;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.Provider;
using System.Collections.Generic;
using Android.Util;
using Com.Github.Barteksc.Pdfviewer;
using Com.Github.Barteksc.Pdfviewer.Listener;
using Com.Github.Barteksc.Pdfviewer.Scroll;
using Com.Github.Barteksc.Pdfviewer.Util;
using Java.Lang;
using Android.Graphics.Pdf;

namespace AndroidPdfViewerSample
{
	[Activity(Label = "fr.ideine.androidpdfviewer.sample",
			  MainLauncher = true,
			  Theme = "@style/Theme.AppCompat.Light")]
	public class MainActivity : AppCompatActivity, IOnPageChangeListener, IOnLoadCompleteListener, IOnPageErrorListener
	{
		private const string TAG = nameof(MainActivity);

		public const int REQUEST_CODE = 42;
		public const int PERMISSION_CODE = 42024;
		public const string SAMPLE_FILE = "sample.pdf";
		public const string READ_EXTERNAL_STORAGE = "android.permission.READ_EXTERNAL_STORAGE";

		private PDFView pdfView;
		private string pdfFileName;
		private Uri uri;
		private int pageNumber;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			pdfView = FindViewById<PDFView>(Resource.Id.pdfView);

			pdfView.SetBackgroundColor(Color.LightGray);
			if (uri != null)
			{
				DisplayFromUri(uri);
			}
			else
			{
				DisplayFromAsset(SAMPLE_FILE);
			}
			Title = pdfFileName;
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == REQUEST_CODE)
			{
				if (resultCode == Result.Ok)
				{
					uri = data.Data;
					DisplayFromUri(uri);
				}
			}
			else
			{
				base.OnActivityResult(requestCode, resultCode, data);
			}
		}

		private void DisplayFromAsset(string assetFileName)
		{
			pdfFileName = assetFileName;

			pdfView.FromAsset(SAMPLE_FILE)
				   .DefaultPage(pageNumber)
				   .OnPageChange(this)
				   .EnableAnnotationRendering(true)
				   .OnLoad(this)
				   .ScrollHandle(new DefaultScrollHandle(this))
				   .Spacing(10) // in dp
				   .OnPageError(this)
				   //.pageFitPolicy(FitPolicy.BOTH)
				   .Load();
		}

		private void DisplayFromUri(Uri uri)
		{
			pdfFileName = GetFileName(uri);

			pdfView.FromUri(uri)
				   .DefaultPage(pageNumber)
				   .OnPageChange(this)
				   .EnableAnnotationRendering(true)
				   .OnLoad(this)
				   .ScrollHandle(new DefaultScrollHandle(this))
				   .Spacing(10) // in dp
				   .OnPageError(this)
				   .Load();
		}

		public string GetFileName(Uri uri)
		{
			string result = null;
			if (uri.Scheme.Equals("content"))
			{
				var cursor = ContentResolver.Query(uri, null, null, null, null);
				try
				{
					if (cursor != null && cursor.MoveToFirst())
					{
						result = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
					}
				}
				finally
				{
					if (cursor != null)
					{
						cursor.Close();
					}
				}
			}
			if (result == null)
			{
				result = uri.LastPathSegment;
			}
			return result;
		}

		//public void PrintBookmarksTree(List<PdfDocument.Bookmark> tree, string sep)
		//{
		//	foreach (PdfDocument.Bookmark b in tree)
		//	{

		//		Log.Error(TAG, string.Format("%s %s, p %d", sep, b.getTitle(), b.getPageIdx()));

		//		if (b.hasChildren())
		//		{
		//			PrintBookmarksTree(b.getChildren(), sep + "-");
		//		}
		//	}
		//}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
		{
			if (requestCode == PERMISSION_CODE)
			{
				if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
				{
					LaunchPicker();
				}
			}
			else
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			}
		}

		private void PickFile()
		{
			var permissionCheck = ContextCompat.CheckSelfPermission(this, READ_EXTERNAL_STORAGE);

			if (permissionCheck != Android.Content.PM.Permission.Granted)
			{
				ActivityCompat.RequestPermissions(this, new string[] { READ_EXTERNAL_STORAGE }, PERMISSION_CODE);

				return;
			}

			LaunchPicker();
		}

		private void LaunchPicker()
		{
			var intent = new Intent(Intent.ActionGetContent);
			intent.SetType("application/pdf");
			try
			{
				StartActivityForResult(intent, REQUEST_CODE);
			}
			catch (ActivityNotFoundException e)
			{
				//alert user that file manager not working
				Toast.MakeText(this, Resource.String.toast_pick_file_error, ToastLength.Short).Show();
			}
		}

		public void OnPageChanged(int p0, int p1)
		{
			pageNumber = p0;
			Title = string.Format("%s %s / %s", pdfFileName, p0 + 1, p1);
		}

		public void LoadComplete(int p0)
		{
			//PdfDocument.Meta meta = pdfView.getDocumentMeta();
			//Log.Error(TAG, "title = " + meta.getTitle());
			//Log.Error(TAG, "author = " + meta.getAuthor());
			//Log.Error(TAG, "subject = " + meta.getSubject());
			//Log.Error(TAG, "keywords = " + meta.getKeywords());
			//Log.Error(TAG, "creator = " + meta.getCreator());
			//Log.Error(TAG, "producer = " + meta.getProducer());
			//Log.Error(TAG, "creationDate = " + meta.getCreationDate());
			//Log.Error(TAG, "modDate = " + meta.getModDate());

			//PrintBookmarksTree(pdfView.getTableOfContents(), "-");

		}

		public void OnPageError(int p0, Throwable p1)
		{
			Log.Error(TAG, "Cannot load page " + p0);
		}
	}
}

