﻿// <auto-generated />
using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(DocumentManagementSystemDbContext))]
    [Migration("20250203023646_AddMethodForResourceDatabase")]
    partial class AddMethodForResourceDatabase
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BusinessObject.ArchivedDocument", b =>
                {
                    b.Property<Guid>("ArchivedDocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("ArchivedDocumentName")
                        .HasColumnType("text");

                    b.Property<string>("ArchivedDocumentUrl")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DocumentTypeId")
                        .HasColumnType("uuid");

                    b.HasKey("ArchivedDocumentId");

                    b.HasIndex("DocumentTypeId");

                    b.ToTable("ArchivedDocument", (string)null);
                });

            modelBuilder.Entity("BusinessObject.AttachmentArchivedDocument", b =>
                {
                    b.Property<Guid>("AttachmentArchivedDocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("ArchivedDocumentId")
                        .HasColumnType("uuid");

                    b.Property<string>("AttachmentName")
                        .HasColumnType("text");

                    b.Property<string>("AttachmentUrl")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("AttachmentArchivedDocumentId");

                    b.HasIndex("ArchivedDocumentId");

                    b.ToTable("AttachmentArchivedDocument", (string)null);
                });

            modelBuilder.Entity("BusinessObject.AttachmentDocument", b =>
                {
                    b.Property<Guid>("AttachmentDocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("AttachmentDocumentName")
                        .HasColumnType("text");

                    b.Property<string>("AttachmentDocumentUrl")
                        .HasColumnType("text");

                    b.Property<Guid>("DocumentFileExtensionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("AttachmentDocumentId");

                    b.HasIndex("DocumentFileExtensionId");

                    b.HasIndex("DocumentId");

                    b.ToTable("AttachmentDocument", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Comment", b =>
                {
                    b.Property<Guid>("CommentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("CommentContent")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("CommentId");

                    b.HasIndex("TaskId");

                    b.HasIndex("UserId");

                    b.ToTable("Comment", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Deadline", b =>
                {
                    b.Property<Guid>("DeadlineId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("ReminderDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("DeadlineId");

                    b.HasIndex("UserId");

                    b.ToTable("Deadline", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Division", b =>
                {
                    b.Property<Guid>("DivisionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("DivisionName")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("DivisionId");

                    b.ToTable("Division", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Document", b =>
                {
                    b.Property<Guid>("DocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeadlineId")
                        .HasColumnType("uuid");

                    b.Property<string>("DocumentCode")
                        .HasColumnType("text");

                    b.Property<Guid>("DocumentFileExtensionId")
                        .HasColumnType("uuid");

                    b.Property<string>("DocumentName")
                        .HasColumnType("text");

                    b.Property<string>("DocumentNumber")
                        .HasColumnType("text");

                    b.Property<int>("DocumentPriority")
                        .HasColumnType("integer");

                    b.Property<int>("DocumentStatus")
                        .HasColumnType("integer");

                    b.Property<Guid>("DocumentTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("DocumentUrl")
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsTemplate")
                        .HasColumnType("boolean");

                    b.HasKey("DocumentId");

                    b.HasIndex("DocumentFileExtensionId");

                    b.HasIndex("DocumentTypeId");

                    b.ToTable("Document", (string)null);
                });

            modelBuilder.Entity("BusinessObject.DocumentFileExtension", b =>
                {
                    b.Property<Guid>("DocumentFileExtensionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("DocumentFileExtensionName")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("DocumentFileExtensionId");

                    b.ToTable("DocumentFileExtension", (string)null);
                });

            modelBuilder.Entity("BusinessObject.DocumentType", b =>
                {
                    b.Property<Guid>("DocumentTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("DocumentTypeName")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("DocumentTypeId");

                    b.ToTable("DocumentType", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Permission", b =>
                {
                    b.Property<Guid>("PermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("PermissionName")
                        .HasColumnType("text");

                    b.HasKey("PermissionId");

                    b.ToTable("Permission", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Resource", b =>
                {
                    b.Property<Guid>("ResourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("ResourceApi")
                        .HasColumnType("text");

                    b.Property<string>("ResourceMethod")
                        .HasColumnType("text");

                    b.Property<string>("ResourceName")
                        .HasColumnType("text");

                    b.HasKey("ResourceId");

                    b.ToTable("Resource", (string)null);
                });

            modelBuilder.Entity("BusinessObject.ResourcePermission", b =>
                {
                    b.Property<Guid>("ResourcePermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("ResourceId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RolePermissionId")
                        .HasColumnType("uuid");

                    b.HasKey("ResourcePermissionId");

                    b.HasIndex("ResourceId");

                    b.HasIndex("RolePermissionId");

                    b.ToTable("ResourcePermission", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Role", b =>
                {
                    b.Property<Guid>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RoleName")
                        .HasColumnType("text");

                    b.Property<Guid>("StepId")
                        .HasColumnType("uuid");

                    b.HasKey("RoleId");

                    b.HasIndex("StepId")
                        .IsUnique();

                    b.ToTable("Role", (string)null);
                });

            modelBuilder.Entity("BusinessObject.RolePermission", b =>
                {
                    b.Property<Guid>("RolePermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("RolePermissionId");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolePermission", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Step", b =>
                {
                    b.Property<Guid>("StepId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Action")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<int>("StepNumber")
                        .HasColumnType("integer");

                    b.Property<Guid>("WorkflowId")
                        .HasColumnType("uuid");

                    b.HasKey("StepId");

                    b.HasIndex("WorkflowId");

                    b.ToTable("Step", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Task", b =>
                {
                    b.Property<Guid>("TaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("StepId")
                        .HasColumnType("uuid");

                    b.Property<int>("TaskStatus")
                        .HasColumnType("integer");

                    b.Property<int>("TaskType")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("TaskId");

                    b.HasIndex("DocumentId");

                    b.HasIndex("StepId");

                    b.ToTable("Task", (string)null);
                });

            modelBuilder.Entity("BusinessObject.TaskUser", b =>
                {
                    b.Property<Guid>("TaskUserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<bool>("IsCreatedTaskByUser")
                        .HasColumnType("boolean");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("TaskUserId");

                    b.HasIndex("RoleId");

                    b.HasIndex("TaskId");

                    b.HasIndex("UserId");

                    b.ToTable("TaskUser", (string)null);
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("Avatar")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DivisionId")
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FcmToken")
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnable")
                        .HasColumnType("boolean");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.Property<int>("VerificationOtpId")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.HasIndex("DivisionId");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("BusinessObject.UserDocument", b =>
                {
                    b.Property<Guid>("UserDocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsCreatedDocumentByUser")
                        .HasColumnType("boolean");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("UserDocumentId");

                    b.HasIndex("DocumentId");

                    b.HasIndex("UserId");

                    b.ToTable("UserDocument", (string)null);
                });

            modelBuilder.Entity("BusinessObject.UserDocumentPermission", b =>
                {
                    b.Property<Guid>("UserDocumentPermissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("ArchivedDocumentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("UserDocumentPermissionId");

                    b.HasIndex("ArchivedDocumentId");

                    b.HasIndex("PermissionId");

                    b.HasIndex("UserId");

                    b.ToTable("UserDocumentPermission", (string)null);
                });

            modelBuilder.Entity("BusinessObject.UserRole", b =>
                {
                    b.Property<Guid>("UserRoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<bool>("IsPrimary")
                        .HasColumnType("boolean");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("UserRoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRole", (string)null);
                });

            modelBuilder.Entity("BusinessObject.VerificationOtp", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsTrue")
                        .HasColumnType("boolean");

                    b.Property<string>("Otp")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("VerificationOtp", (string)null);
                });

            modelBuilder.Entity("BusinessObject.Workflow", b =>
                {
                    b.Property<Guid>("WorkflowId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("DocumentTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("WorkflowName")
                        .HasColumnType("text");

                    b.HasKey("WorkflowId");

                    b.HasIndex("DocumentTypeId");

                    b.ToTable("Workflow", (string)null);
                });

            modelBuilder.Entity("BusinessObject.ArchivedDocument", b =>
                {
                    b.HasOne("BusinessObject.DocumentType", "DocumentType")
                        .WithMany("ArchivedDocuments")
                        .HasForeignKey("DocumentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DocumentType");
                });

            modelBuilder.Entity("BusinessObject.AttachmentArchivedDocument", b =>
                {
                    b.HasOne("BusinessObject.ArchivedDocument", "ArchivedDocument")
                        .WithMany("AttachmentArchivedDocuments")
                        .HasForeignKey("ArchivedDocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ArchivedDocument");
                });

            modelBuilder.Entity("BusinessObject.AttachmentDocument", b =>
                {
                    b.HasOne("BusinessObject.DocumentFileExtension", "DocumentFileExtension")
                        .WithMany("AttachmentDocuments")
                        .HasForeignKey("DocumentFileExtensionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Document", "Document")
                        .WithMany("AttachmentDocuments")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("DocumentFileExtension");
                });

            modelBuilder.Entity("BusinessObject.Comment", b =>
                {
                    b.HasOne("BusinessObject.Task", "Task")
                        .WithMany("Comments")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.Deadline", b =>
                {
                    b.HasOne("BusinessObject.Document", "Document")
                        .WithOne("Deadline")
                        .HasForeignKey("BusinessObject.Deadline", "DeadlineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("Deadlines")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.Document", b =>
                {
                    b.HasOne("BusinessObject.DocumentFileExtension", "DocumentFileExtension")
                        .WithMany("Documents")
                        .HasForeignKey("DocumentFileExtensionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.DocumentType", "DocumentType")
                        .WithMany("Documents")
                        .HasForeignKey("DocumentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DocumentFileExtension");

                    b.Navigation("DocumentType");
                });

            modelBuilder.Entity("BusinessObject.ResourcePermission", b =>
                {
                    b.HasOne("BusinessObject.Resource", "Resource")
                        .WithMany("ResourcePermissions")
                        .HasForeignKey("ResourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.RolePermission", "RolePermission")
                        .WithMany("ResourcePermissions")
                        .HasForeignKey("RolePermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Resource");

                    b.Navigation("RolePermission");
                });

            modelBuilder.Entity("BusinessObject.Role", b =>
                {
                    b.HasOne("BusinessObject.Step", "Step")
                        .WithOne("Role")
                        .HasForeignKey("BusinessObject.Role", "StepId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Step");
                });

            modelBuilder.Entity("BusinessObject.RolePermission", b =>
                {
                    b.HasOne("BusinessObject.Permission", "Permission")
                        .WithMany("RolePermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Role", "Role")
                        .WithMany("RolePermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BusinessObject.Step", b =>
                {
                    b.HasOne("BusinessObject.Workflow", "Workflow")
                        .WithMany("Steps")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("BusinessObject.Task", b =>
                {
                    b.HasOne("BusinessObject.Document", "Document")
                        .WithMany("Tasks")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Step", "Step")
                        .WithMany("Tasks")
                        .HasForeignKey("StepId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("Step");
                });

            modelBuilder.Entity("BusinessObject.TaskUser", b =>
                {
                    b.HasOne("BusinessObject.Role", "Role")
                        .WithMany("TaskUsers")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Task", "Task")
                        .WithMany("TaskUsers")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("TaskUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Task");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.HasOne("BusinessObject.Division", "Division")
                        .WithMany("Users")
                        .HasForeignKey("DivisionId");

                    b.Navigation("Division");
                });

            modelBuilder.Entity("BusinessObject.UserDocument", b =>
                {
                    b.HasOne("BusinessObject.Document", "Document")
                        .WithMany("UserDocuments")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("UserDocuments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.UserDocumentPermission", b =>
                {
                    b.HasOne("BusinessObject.ArchivedDocument", "ArchivedDocument")
                        .WithMany("UserDocumentPermissions")
                        .HasForeignKey("ArchivedDocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.Permission", "Permission")
                        .WithMany("UserDocumentPermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("UserDocumentPermissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ArchivedDocument");

                    b.Navigation("Permission");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.UserRole", b =>
                {
                    b.HasOne("BusinessObject.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.VerificationOtp", b =>
                {
                    b.HasOne("BusinessObject.User", "User")
                        .WithOne("VerificationOtp")
                        .HasForeignKey("BusinessObject.VerificationOtp", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.Workflow", b =>
                {
                    b.HasOne("BusinessObject.DocumentType", "DocumentType")
                        .WithMany("Workflows")
                        .HasForeignKey("DocumentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DocumentType");
                });

            modelBuilder.Entity("BusinessObject.ArchivedDocument", b =>
                {
                    b.Navigation("AttachmentArchivedDocuments");

                    b.Navigation("UserDocumentPermissions");
                });

            modelBuilder.Entity("BusinessObject.Division", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("BusinessObject.Document", b =>
                {
                    b.Navigation("AttachmentDocuments");

                    b.Navigation("Deadline");

                    b.Navigation("Tasks");

                    b.Navigation("UserDocuments");
                });

            modelBuilder.Entity("BusinessObject.DocumentFileExtension", b =>
                {
                    b.Navigation("AttachmentDocuments");

                    b.Navigation("Documents");
                });

            modelBuilder.Entity("BusinessObject.DocumentType", b =>
                {
                    b.Navigation("ArchivedDocuments");

                    b.Navigation("Documents");

                    b.Navigation("Workflows");
                });

            modelBuilder.Entity("BusinessObject.Permission", b =>
                {
                    b.Navigation("RolePermissions");

                    b.Navigation("UserDocumentPermissions");
                });

            modelBuilder.Entity("BusinessObject.Resource", b =>
                {
                    b.Navigation("ResourcePermissions");
                });

            modelBuilder.Entity("BusinessObject.Role", b =>
                {
                    b.Navigation("RolePermissions");

                    b.Navigation("TaskUsers");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("BusinessObject.RolePermission", b =>
                {
                    b.Navigation("ResourcePermissions");
                });

            modelBuilder.Entity("BusinessObject.Step", b =>
                {
                    b.Navigation("Role");

                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("BusinessObject.Task", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("TaskUsers");
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Deadlines");

                    b.Navigation("TaskUsers");

                    b.Navigation("UserDocumentPermissions");

                    b.Navigation("UserDocuments");

                    b.Navigation("UserRoles");

                    b.Navigation("VerificationOtp");
                });

            modelBuilder.Entity("BusinessObject.Workflow", b =>
                {
                    b.Navigation("Steps");
                });
#pragma warning restore 612, 618
        }
    }
}
