﻿using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Binaris
{
	public class CollageBinario:ElementoIEnumerableBinario
	{
		public CollageBinario() : base(new ImageFragmentBinario(),LongitudBinaria.UInt)
		{

		}
		public override byte[] GetBytes(object obj)
		{
			if (obj is Collage)
			{
				Longitud = LongitudBinaria.UInt;
			}
			return base.GetBytes(obj);
		}
		public override object GetObject(MemoryStream bytes)
		{
			return new Collage(((object[])base.GetObject(bytes)).Casting<ImageFragment>(false));
		}
	}
	public class ImageFragmentBinario : ElementoComplejoBinario
	{
		public ImageFragmentBinario()
		{
			base.PartesElemento.Add(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.PointZ));
			base.PartesElemento.Add(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Bitmap));
		}
		public override byte[] GetBytes(object obj)
		{
			List<byte> bytesObj = new List<byte>();
			ImageFragment fragment= obj as ImageFragment;
			if (fragment!=null)
			{
				bytesObj.AddRange(PartesElemento[0].GetBytes(fragment.Location));
				bytesObj.AddRange(PartesElemento[1].GetBytes(fragment.Image));
			}
			else {
				bytesObj.Add(0x00);
			}
			return bytesObj.ToArray();
		}

		protected override object GetObject(object[] parts)
		{
			PointZ location = (PointZ)parts[0];
			Bitmap bmp = null;
			ImageFragment fragment = null;
			
			bmp = parts[1] as Bitmap;
			
			fragment = new ImageFragment(bmp, location);
			return fragment;
		}

	}
}
