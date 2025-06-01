using CompanyVacancy.Models;

namespace CompanyVacancy.Helpers
{
    public class Sorting
    {
       
            public static void MergeSort(CompanyDetails[] arr, int left, int right)
            {
                if (left < right)
                {
                    int mid = (left + right) / 2;
                    MergeSort(arr, left, mid);
                    MergeSort(arr, mid + 1, right);
                    Merge(arr, left, mid, right);
                }
            }

            private static void Merge(CompanyDetails[] arr, int left, int mid, int right)
            {
                int n1 = mid - left + 1;
                int n2 = right - mid;

               
                CompanyDetails[] leftArray = new CompanyDetails[n1];
                CompanyDetails[] rightArray = new CompanyDetails[n2];

                for (int i = 0; i < n1; i++)
                    leftArray[i] = arr[left + i];
                for (int j = 0; j < n2; j++)
                    rightArray[j] = arr[mid + 1 + j];

                int a = 0, b = 0, k = left;

              
                while (a < n1 && b < n2)
                {
                    
                    if (leftArray[a].Vacancies <= rightArray[b].Vacancies)
                    {
                        arr[k] = leftArray[a];
                        a++;
                    }
                    else
                    {
                        arr[k] = rightArray[b];
                        b++;
                    }
                    k++;
                }

               
                while (a < n1)
                {
                    arr[k] = leftArray[a];
                    a++;
                    k++;
                }

                
                while (b < n2)
                {
                    arr[k] = rightArray[b];
                    b++;
                    k++;
                }
            }
        

        public static void Display(List<int> A)
        {
            foreach (var item in A)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();
        }

    }
}
