using Bot_Book;
using System.Text;
using System.Threading.Tasks;

Console.InputEncoding = Encoding.Unicode;
Console.OutputEncoding = Encoding.Unicode;

await new BookSearch124_Bot().Start();

await Task.Delay(-1);
