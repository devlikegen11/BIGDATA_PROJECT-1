#include <iostream>
#include <cstdlib>
#include <unistd.h>
#include <cstring>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <pthread.h>
#include <vector>
#include <mariadb/conncpp.hpp>
#include <nlohmann/json.hpp>
#include <unordered_map>
#include <iomanip>
#include <sstream>
#include <curl/curl.h>

#define BUF_SIZE 1024
#define MAX_CLNT 256

using json = nlohmann::json;

void *handle_clnt(void *arg);
void send_msg(const char *msg, int len, int sender_sock, std::string id);
void error_handling(const char *msg);

int clnt_cnt = 0;
int clnt_socks[MAX_CLNT];
std::string user_id;
pthread_mutex_t mutx;

int c_cli = 0;
int py_cli = 0;

class DB
{
protected:
  sql::Connection *conn;

public:
  void connect()
  {
    try
    {
      sql::Driver *driver = sql::mariadb::get_driver_instance();
      sql::SQLString url("jdbc:mariadb://10.10.21.115:3306/AITRAIN");
      sql::Properties properties({{"user", "AI"}, {"password", "1234"}});
      conn = driver->connect(url, properties);
    }
    catch (sql::SQLException &e)
    {
      std::cerr << "Error Connecting to MariaDB Platform: " << e.what() << std::endl;
    }
  }

  bool attemptLogin(const std::string &ID, const std::string &PW)
  {
    try
    {
      std::cout << "유저 접속어탬프" << std::endl;
      std::unique_ptr<sql::Statement> stmnt(conn->createStatement());
      sql::ResultSet *res = stmnt->executeQuery("SELECT * FROM USER");
      while (res->next())
      {
        if (res->getString(2) == ID && res->getString(3) == PW)
        {
          return true;
        }
      }
    }
    catch (sql::SQLException &e)
    {
      std::cerr << "Error during login attempt: " << e.what() << std::endl;
    }
    return false;
  }

  void USERJOIN(std::string ID, std::string PW, int PERSONAL)
  {
    try
    {
      std::cout << "회원가입 진행" << std::endl;
      std::unique_ptr<sql::PreparedStatement> stmnt(conn->prepareStatement("INSERT INTO USER(USER_ID, USER_PW, USER_PERSONAL) VALUES (?, ?, ?);"));
      stmnt->setString(1, ID);
      stmnt->setString(2, PW);
      stmnt->setInt(3, PERSONAL);
      stmnt->executeQuery();
    }
    catch (const std::exception &e)
    {
      std::cerr << "회원가입 에러: " << e.what() << std::endl;
    }
  }

  void SOLINSERT(std::string with_stream, std::string no_stream)
  {
    try
    {
      std::unique_ptr<sql::PreparedStatement> stmnt(conn->prepareStatement("INSERT INTO SOLUTION(STREAM_YES, STREAM_NO) VALUES (?, ?);"));
      stmnt->setString(1, with_stream);
      stmnt->setString(2, no_stream);
      stmnt->executeQuery();
    }
    catch (const std::exception &e)
    {
      std::cerr << "솔루션 삽입 에러" << e.what() << '\n';
    }
  }

  json SHOWSOL()
  {
    json testset;
    std::vector<json> test_q;
    try
    {
      std::unique_ptr<sql::PreparedStatement> stmnt(conn->prepareStatement("SELECT STREAM_YES, STREAM_NO FROM SOLUTION;"));
      std::unique_ptr<sql::ResultSet> res6(stmnt->executeQuery());

      while (res6->next())
      {
        json test_cord;
        test_cord["STREAM_YES"] = res6->getString("STREAM_YES");
        test_cord["STREAM_NO"] = res6->getString("STREAM_NO");
        test_q.emplace_back(test_cord);
      }
      testset["TEST_GO"] = test_q;
    }
    catch (sql::SQLException &e)
    {
      std::cerr << "Error fetching user info: " << e.what() << std::endl;
    }
    return testset;
  }

  ~DB() { delete conn; }
};

void *handle_clnt(void *arg)
{
  int clnt_sock = *((int *)arg);
  int str_len = 0;
  int py_search = 0;
  char msg[BUF_SIZE];
  bool is_user = false;
  bool is_consultant = false;
  int mapped_cs_sock = -1;
  std::cout << "핸들" << std::endl;
  DB db;
  db.connect();

  memset(msg, 0, BUF_SIZE);
  str_len = read(clnt_sock, msg, sizeof(msg));
  std::cout << msg << std::endl;

  json conn = json::parse(std::string(msg, str_len));
  std::string first_conn = conn["first_msg"];
  std::cout << first_conn << std::endl;
  memset(msg, 0, BUF_SIZE);

  if (first_conn == "10")
  {
    std::cout << "c#클라이언트" << std::endl;
    c_cli = clnt_sock;

    while ((str_len = read(clnt_sock, msg, sizeof(msg))) != 0)
    {
      json received_json = json::parse(std::string(msg, str_len));
      std::string client_type = received_json["type"];

      char *buf = nullptr;
      int len = 0;

      if (client_type == "USER")
      {
        user_id = received_json["id"];
        std::string pw = received_json["password"];
        bool login_success = db.attemptLogin(user_id, pw);

        json response;
        response["login_success"] = login_success;
        std::string response_str = response.dump();
        std::cout << response_str << std::endl;
        write(clnt_sock, response_str.c_str(), response_str.length());

        memset(msg, 0, BUF_SIZE);
        std::cout << "검색" << std::endl;

        int py_search = read(clnt_sock, msg, sizeof(msg) - 1); // 검색하고 싶은 정보가 들어옴
        msg[py_search] = '\0';                                 // 문자열 종료 문자 추가
        std::cout << "검색 정보: " << msg << std::endl;
        write(py_cli, msg, py_search); // 파이썬에 검색 정보 전송

        sleep(1);
        json send_test = db.SHOWSOL();
        std::string test_str = send_test.dump();
        std::cout << test_str << std::endl;
        write(clnt_sock, test_str.c_str(), test_str.length());
      }
      else if (client_type == "JOIN")
      {
        std::cout << "회원가입" << std::endl;
        memset(msg, 0, BUF_SIZE);
        str_len = read(clnt_sock, msg, sizeof(msg) - 1);
        json join_plz = json::parse(std::string(msg, str_len));
        std::string ID = join_plz["id"];
        std::string PW = join_plz["pw"];
        std::string pw_check = join_plz["pw_check"];
        int personal = join_plz["person"];

        db.USERJOIN(ID, PW, personal);
      }
    }
  }
  else if (first_conn == "11")
  {
    std::cout << "py클라이언트" << std::endl;
    py_cli = clnt_sock;
    while (true)
    {
      str_len = read(clnt_sock, msg, sizeof(msg)); // 검색 결과 수신
      std::cout << "결과확인\n"
                << msg << std::endl;

      json received_search = json::parse(std::string(msg, str_len));
      std::string no_stream = std::to_string(received_search["no_stream"].get<float>());
      std::string with_stream = std::to_string(received_search["with_stream"].get<float>());
      std::cout << no_stream << std::endl;
      std::cout << with_stream << std::endl;
      // db.SOLINSERT(with_stream, no_stream);
      write(c_cli, msg, str_len);
      std::cout << "전송" << std::endl;
    }
  }

  pthread_mutex_lock(&mutx);
  for (int i = 0; i < clnt_cnt; i++)
  {
    if (clnt_sock == clnt_socks[i])
    {
      while (i++ < clnt_cnt - 1)
        clnt_socks[i] = clnt_socks[i + 1];
      break;
    }
  }
  clnt_cnt--;
  pthread_mutex_unlock(&mutx);
  close(clnt_sock);
  return NULL;
}

void send_msg(const char *msg, int len) // send to all
{
  int i;
  pthread_mutex_lock(&mutx);
  for (i = 0; i < clnt_cnt; i++)
    write(clnt_socks[i], msg, len);
  pthread_mutex_unlock(&mutx);
}

void error_handling(const char *msg)
{
  fputs(msg, stderr);
  fputc('\n', stderr);
  exit(1);
}

int main(int argc, char *argv[])
{
  std::cout << "서버 시작" << std::endl;
  int serv_sock, clnt_sock;
  struct sockaddr_in serv_adr, clnt_adr;
  socklen_t clnt_adr_sz;
  pthread_t t_id;

  if (argc != 2)
  {
    printf("Usage : %s <port>\n", argv[0]);
    exit(1);
  }

  pthread_mutex_init(&mutx, NULL);
  serv_sock = socket(PF_INET, SOCK_STREAM, 0);

  memset(&serv_adr, 0, sizeof(serv_adr));
  serv_adr.sin_family = AF_INET;
  serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
  serv_adr.sin_port = htons(atoi(argv[1]));

  if (bind(serv_sock, (struct sockaddr *)&serv_adr, sizeof(serv_adr)) == -1)
    error_handling("bind() error");
  if (listen(serv_sock, 5) == -1)
    error_handling("listen() error");

  while (1)
  {
    clnt_adr_sz = sizeof(clnt_adr);
    clnt_sock = accept(serv_sock, (struct sockaddr *)&clnt_adr, &clnt_adr_sz);
    pthread_mutex_lock(&mutx);
    clnt_socks[clnt_cnt++] = clnt_sock;
    pthread_mutex_unlock(&mutx);

    pthread_create(&t_id, NULL, handle_clnt, (void *)&clnt_sock);
    pthread_detach(t_id);
    printf("Connected client IP: %s \n", inet_ntoa(clnt_adr.sin_addr));
  }
  close(serv_sock);
  return 0;
}