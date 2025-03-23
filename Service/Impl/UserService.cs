using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject;
using DataAccess.DAO;
using DataAccess.DTO;
using DataAccess.DTO.Request;
using Npgsql.Replication;
using Repository;
using Service.Response;
using Service.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Service.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }


    
}