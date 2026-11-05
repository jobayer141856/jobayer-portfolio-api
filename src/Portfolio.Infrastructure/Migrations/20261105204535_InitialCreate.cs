using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contact_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    sender_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    message_body = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ip_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "educations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    degree = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    result_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    show_result_publicly = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    institution_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_educations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    public_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    alt_text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, defaultValue: ""),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skill_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_admin_users_admin_user_id",
                        column: x => x.admin_user_id,
                        principalTable: "admin_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_markdown = table.Column<string>(type: "text", nullable: false),
                    cover_image_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    reading_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_articles_media_assets_cover_image_asset_id",
                        column: x => x.cover_image_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    credential_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    credential_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    badge_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_certifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_certifications_media_assets_badge_asset_id",
                        column: x => x.badge_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "experiences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    organization_logo_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    employment_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    work_arrangement = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    responsibilities = table.Column<string>(type: "text", nullable: false),
                    outcomes = table.Column<string>(type: "text", nullable: false),
                    technology_tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_experiences", x => x.id);
                    table.ForeignKey(
                        name: "fk_experiences_media_assets_organization_logo_asset_id",
                        column: x => x.organization_logo_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    headline = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    short_intro = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    biography_markdown = table.Column<string>(type: "text", nullable: false),
                    working_approach_markdown = table.Column<string>(type: "text", nullable: true),
                    public_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    show_email_publicly = table.Column<bool>(type: "boolean", nullable: false),
                    profile_image_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resume_pdf_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    seo_title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    social_links_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_profiles_media_assets_profile_image_asset_id",
                        column: x => x.profile_image_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_profiles_media_assets_resume_pdf_asset_id",
                        column: x => x.resume_pdf_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    short_summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    problem_context_markdown = table.Column<string>(type: "text", nullable: true),
                    goals_markdown = table.Column<string>(type: "text", nullable: true),
                    role_contributions_markdown = table.Column<string>(type: "text", nullable: true),
                    architecture_overview_markdown = table.Column<string>(type: "text", nullable: true),
                    challenges_decisions_markdown = table.Column<string>(type: "text", nullable: true),
                    outcomes_markdown = table.Column<string>(type: "text", nullable: true),
                    technology_tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    cover_image_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    live_demo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    repository_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    seo_title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_media_assets_cover_image_asset_id",
                        column: x => x.cover_image_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "publications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    venue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    publication_year = table.Column<int>(type: "integer", nullable: false),
                    publication_date = table.Column<DateOnly>(type: "date", nullable: true),
                    doi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    official_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pdf_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    abstract_text = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    authors_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publications", x => x.id);
                    table.ForeignKey(
                        name: "fk_publications_media_assets_pdf_asset_id",
                        column: x => x.pdf_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "skill_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    context_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skill_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_skill_items_skill_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "skill_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_images_media_assets_media_asset_id",
                        column: x => x.media_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_images_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_users_email",
                table: "admin_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_articles_cover_image_asset_id",
                table: "blog_articles",
                column: "cover_image_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_articles_slug",
                table: "blog_articles",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_certifications_badge_asset_id",
                table: "certifications",
                column: "badge_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_experiences_organization_logo_asset_id",
                table: "experiences",
                column: "organization_logo_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_storage_key",
                table: "media_assets",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_profiles_profile_image_asset_id",
                table: "profiles",
                column: "profile_image_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_profiles_resume_pdf_asset_id",
                table: "profiles",
                column: "resume_pdf_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_images_media_asset_id",
                table: "project_images",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_images_project_id",
                table: "project_images",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_cover_image_asset_id",
                table: "projects",
                column: "cover_image_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_slug",
                table: "projects",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_publications_pdf_asset_id",
                table: "publications",
                column: "pdf_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_publications_slug",
                table: "publications",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_admin_user_id",
                table: "refresh_tokens",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skill_items_category_id",
                table: "skill_items",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_articles");

            migrationBuilder.DropTable(
                name: "certifications");

            migrationBuilder.DropTable(
                name: "contact_messages");

            migrationBuilder.DropTable(
                name: "educations");

            migrationBuilder.DropTable(
                name: "experiences");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "project_images");

            migrationBuilder.DropTable(
                name: "publications");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "skill_items");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "skill_categories");

            migrationBuilder.DropTable(
                name: "media_assets");
        }
    }
}
