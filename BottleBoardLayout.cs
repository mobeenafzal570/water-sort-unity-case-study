using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Fits the actual glass artwork inside the play area, reserving space for the receiver.</summary>
public static class BottleBoardLayout
{
    public const float HorizontalGap=20f;
    public const float VerticalGap=64f;
    public static Rect Bounds(Bottle bottle,RectTransform parent)
    {
        var front=bottle.GetComponentsInChildren<UnityEngine.UI.Image>(true).FirstOrDefault(i=>i.name=="BottleFront");
        var rect=front!=null?front.rectTransform:(RectTransform)bottle.transform;
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        Vector2 low=parent.InverseTransformPoint(corners[0]),high=low;
        foreach(var corner in corners){Vector2 p=parent.InverseTransformPoint(corner);low=Vector2.Min(low,p);high=Vector2.Max(high,p);}
        return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
    }
    public static Rect SafeArea(RectTransform parent)
    {
        var canvas=parent.GetComponentInParent<Canvas>();Rect board=parent.rect;
        if(canvas!=null)
        {
            var corners=new Vector3[4];((RectTransform)canvas.transform).GetWorldCorners(corners);
            Vector2 low=parent.InverseTransformPoint(corners[0]),high=parent.InverseTransformPoint(corners[2]);
            board=Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
        float bottom=board.yMin+board.height*.22f;
        var powers=Object.FindObjectsByType<PowerUpManager>(FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene==parent.gameObject.scene);
        if(powers!=null&&powers.undoButton!=null&&powers.shuffleButton!=null)
        {
            Transform common=powers.undoButton.transform.parent;
            while(common!=null&&!powers.shuffleButton.transform.IsChildOf(common))common=common.parent;
            if(common is RectTransform bar&&canvas!=null&&common!=canvas.transform)
            {
                var corners=new Vector3[4];bar.GetWorldCorners(corners);
                float barTop=corners.Max(c=>parent.InverseTransformPoint(c).y);
                if(barTop<board.center.y)bottom=Mathf.Max(bottom,barTop+24f);
            }
        }
        return Rect.MinMaxRect(board.xMin+board.width*.07f,bottom,
            board.xMin+board.width*.93f,board.yMin+board.height*.82f);
    }
    public static bool IsSafe(IList<Bottle> bottles,RectTransform parent,Rect safe)
    {
        for(int i=0;i<bottles.Count;i++)
        {
            Rect a=Bounds(bottles[i],parent);
            if(a.xMin<safe.xMin-.1f||a.xMax>safe.xMax+.1f||a.yMin<safe.yMin-.1f||a.yMax>safe.yMax+.1f)return false;
            for(int j=i+1;j<bottles.Count;j++)
            {
                Rect padded=a;padded.xMin-=HorizontalGap*.45f;padded.xMax+=HorizontalGap*.45f;
                padded.yMin-=VerticalGap*.45f;padded.yMax+=VerticalGap*.45f;
                if(padded.Overlaps(Bounds(bottles[j],parent)))return false;
            }
        }
        return true;
    }
    public static void Fit(Bottle[] bottles,bool force=false)
    {
        if(bottles==null||bottles.Length==0)return;
        var parent=bottles[0].transform.parent as RectTransform;if(parent==null)return;
        Canvas.ForceUpdateCanvases();Rect safe=SafeArea(parent);
        var receiver=bottles.FirstOrDefault(b=>b.fixedColorReceiver);
        if(receiver!=null){FitReceiver(bottles,receiver,parent,safe);return;}
        if(!force&&IsSafe(bottles,parent,safe))return;
        var ordered=bottles.OrderByDescending(b=>Bounds(b,parent).center.y).ThenBy(b=>Bounds(b,parent).center.x).ToArray();
        float width=bottles.Max(b=>Bounds(b,parent).width),height=bottles.Max(b=>Bounds(b,parent).height);
        var rows=new List<List<Bottle>>();
        foreach(var bottle in ordered)
        {
            var row=rows.FirstOrDefault(r=>Mathf.Abs(Bounds(r[0],parent).center.y-Bounds(bottle,parent).center.y)<height*.4f);
            if(row==null){row=new List<Bottle>();rows.Add(row);}row.Add(bottle);
        }
        int columns=rows.Max(r=>r.Count);
        float fit=Mathf.Min(1,(safe.width-(columns-1)*HorizontalGap)/(columns*width),
            (safe.height-(rows.Count-1)*VerticalGap)/(rows.Count*height));
        float[] centers=rows.Select(r=>r.Average(b=>Bounds(b,parent).center.y)).ToArray();
        float fittedHeight=height*fit;
        centers[0]=Mathf.Min(centers[0],safe.yMax-fittedHeight*.5f);
        for(int i=1;i<rows.Count;i++)centers[i]=Mathf.Min(centers[i],centers[i-1]-fittedHeight-VerticalGap);
        centers[centers.Length-1]=Mathf.Max(centers[centers.Length-1],safe.yMin+fittedHeight*.5f);
        for(int i=centers.Length-2;i>=0;i--)centers[i]=Mathf.Max(centers[i],centers[i+1]+fittedHeight+VerticalGap);
        for(int i=0;i<rows.Count;i++)
        {
            rows[i].Sort((a,b)=>Bounds(a,parent).center.x.CompareTo(Bounds(b,parent).center.x));
            for(int j=0;j<rows[i].Count;j++)
            {
                var bottle=rows[i][j];bottle.transform.localScale*=fit;
                Place(bottle,parent,new Vector2(safe.center.x+(j-(rows[i].Count-1)*.5f)*(width*fit+HorizontalGap),centers[i]));
            }
        }
    }
    private static void FitReceiver(Bottle[] bottles,Bottle receiver,RectTransform parent,Rect safe)
    {
        Rect big=Bounds(receiver,parent);
        float receiverFit=Mathf.Min(safe.width*.52f/big.width,safe.height*.46f/big.height);
        receiver.transform.localScale*=receiverFit;Place(receiver,parent,safe.center);big=Bounds(receiver,parent);
        var small=bottles.Where(b=>b!=receiver).OrderBy(b=>b.name,System.StringComparer.Ordinal).ToArray();
        if(small.Length==0)return;
        bool randomize=Application.isPlaying;
        if(randomize)
            for(int i=small.Length-1;i>0;i--)
            {
                int other=Random.Range(0,i+1);
                Bottle swap=small[i];small[i]=small[other];small[other]=swap;
            }
        int sideCount=Mathf.Min(2,small.Length/4);
        int remaining=small.Length-sideCount*2;
        int[] counts={Mathf.CeilToInt(remaining/2f),remaining/2,sideCount,sideCount};
        float middleHeight=Mathf.Max(big.height,(safe.height-VerticalGap)*.5f);
        float middleBottom=safe.center.y-middleHeight*.5f,middleTop=safe.center.y+middleHeight*.5f;
        Rect[] areas={
            Rect.MinMaxRect(safe.xMin,middleTop+VerticalGap,safe.xMax,safe.yMax),
            Rect.MinMaxRect(safe.xMin,safe.yMin,safe.xMax,middleBottom-VerticalGap),
            Rect.MinMaxRect(safe.xMin,middleBottom,big.xMin-HorizontalGap,middleTop),
            Rect.MinMaxRect(big.xMax+HorizontalGap,middleBottom,safe.xMax,middleTop)
        };
        var cells=new List<Rect>();
        for(int side=0;side<areas.Length;side++)
        {
            int count=counts[side];if(count==0)continue;
            Rect area=areas[side];
            bool horizontal=side<2;
            float cellWidth=horizontal?(area.width-(count-1)*HorizontalGap)/count:area.width;
            float cellHeight=horizontal?area.height:(area.height-(count-1)*VerticalGap)/count;
            for(int i=0;i<count;i++)
                cells.Add(new Rect(area.xMin+(horizontal?i*(cellWidth+HorizontalGap):0),
                    area.yMin+(horizontal?0:i*(cellHeight+VerticalGap)),cellWidth,cellHeight));
        }
        float width=small.Max(b=>Bounds(b,parent).width),height=small.Max(b=>Bounds(b,parent).height);
        float fit=Mathf.Min(big.height*.46f/height,
            cells.Min(cell=>Mathf.Min(cell.width*.90f/width,cell.height*.90f/height)));
        for(int i=0;i<small.Length;i++)
        {
            small[i].transform.localScale*=fit;
            Rect cell=cells[i],bounds=Bounds(small[i],parent);
            Vector2 slack=new Vector2(cell.width-bounds.width,cell.height-bounds.height)*.45f;
            Vector2 jitter=randomize?new Vector2(Random.Range(-slack.x,slack.x),Random.Range(-slack.y,slack.y)):Vector2.zero;
            Place(small[i],parent,cell.center+jitter);
        }
    }
    private static void Place(Bottle bottle,RectTransform parent,Vector2 center)
    {bottle.transform.localPosition+=(Vector3)(center-Bounds(bottle,parent).center);}
}
