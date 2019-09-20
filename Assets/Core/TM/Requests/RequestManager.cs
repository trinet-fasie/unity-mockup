using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TM.WWW
{
    public class RequestManager : MonoBehaviour
    {
        private static List<Request> _requestQueue = new List<Request>();
        private static readonly List<Request> RequestQueueToAdd = new List<Request>();
        public bool WorkingWithRequests;
        private static int _requestCouner;
        public static RequestManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!WorkingWithRequests && (_requestQueue.Count > 0 || RequestQueueToAdd.Count > 0))
                StartCoroutine(SendRequests());

            //ToDo Просится положить в ECS
            //Сейчас используется только в запросе на загрузку (RequstDownLoad)
            foreach (Request request in _requestQueue)
            {
                request.OnUpdate?.Invoke();
            }
        }

        public static void AddRequest(Request request)
        {
            request.Number = _requestCouner;
            RequestQueueToAdd.Add(request);
            _requestCouner++;
        }

        IEnumerator SendRequests()
        {
            WorkingWithRequests = true;
            _requestQueue.AddRange(RequestQueueToAdd);
            _requestQueue = _requestQueue.OrderBy(request => request.Number).ToList();
            RequestQueueToAdd.Clear();

            foreach (Request request in _requestQueue)
            {
                if (request.Done) continue;
                IRequest r = request;
                yield return StartCoroutine(r.SendRequest());
            }

            List<Request> toRemoveList = new List<Request>();
            foreach (Request request in _requestQueue)
            {
                if (request.Done)
                    toRemoveList.Add(request);
                if (request.Error)
                {
                    WorkingWithRequests = false;
                    toRemoveList.Add(request);
                }

            }

            foreach (Request request in toRemoveList)
            {
                _requestQueue.Remove(request);
            }

            WorkingWithRequests = false;
            yield return true;
        }

        


    }

}






