#include <cstdint>

namespace CesiumForUnityNative {
/*
Branchless float32->float16 conversion code adapted from
https://github.com/fengwang/float16_t.
Used under a BSD license:

Copyright <2020> <Feng Wang>

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace {
constexpr inline std::uint32_t
_uint32_sels(std::uint32_t test, std::uint32_t a, std::uint32_t b) noexcept {
  const std::uint32_t mask = (((std::int32_t)test) >> 31);
  const std::uint32_t sel_a = (a & mask);
  const std::uint32_t sel_b = (b & ~mask);
  const std::uint32_t result = (sel_a | sel_b);
  return (result);
}

} // namespace

constexpr inline std::uint16_t float_to_half(std::uint32_t f) noexcept {
  const std::uint32_t one = (0x00000001);
  const std::uint32_t f_s_mask = (0x80000000);
  const std::uint32_t f_e_mask = (0x7f800000);
  const std::uint32_t f_m_mask = (0x007fffff);
  const std::uint32_t f_m_hidden_bit = (0x00800000);
  const std::uint32_t f_m_round_bit = (0x00001000);
  const std::uint32_t f_snan_mask = (0x7fc00000);
  const std::uint32_t f_e_pos = (0x00000017);
  const std::uint32_t h_e_pos = (0x0000000a);
  const std::uint32_t h_e_mask = (0x00007c00);
  const std::uint32_t h_snan_mask = (0x00007e00);
  const std::uint32_t h_e_mask_value = (0x0000001f);
  const std::uint32_t f_h_s_pos_offset = (0x00000010);
  const std::uint32_t f_h_bias_offset = (0x00000070);
  const std::uint32_t f_h_m_pos_offset = (0x0000000d);
  const std::uint32_t h_nan_min = (0x00007c01);
  const std::uint32_t f_h_e_biased_flag = (0x0000008f);
  const std::uint32_t f_s = (f & f_s_mask);
  const std::uint32_t f_e = (f & f_e_mask);
  const std::uint16_t h_s = (f_s >> f_h_s_pos_offset);
  const std::uint32_t f_m = (f & f_m_mask);
  const std::uint16_t f_e_amount = (f_e >> f_e_pos);
  const std::uint32_t f_e_half_bias = (f_e_amount - f_h_bias_offset);
  const std::uint32_t f_snan = (f & f_snan_mask);
  const std::uint32_t f_m_round_mask = (f_m & f_m_round_bit);
  const std::uint32_t f_m_round_offset = (f_m_round_mask << one);
  const std::uint32_t f_m_rounded = (f_m + f_m_round_offset);
  const std::uint32_t f_m_denorm_sa = (one - f_e_half_bias);
  const std::uint32_t f_m_with_hidden = (f_m_rounded | f_m_hidden_bit);
  const std::uint32_t f_m_denorm = (f_m_with_hidden >> f_m_denorm_sa);
  const std::uint32_t h_m_denorm = (f_m_denorm >> f_h_m_pos_offset);
  const std::uint32_t f_m_rounded_overflow = (f_m_rounded & f_m_hidden_bit);
  const std::uint32_t m_nan = (f_m >> f_h_m_pos_offset);
  const std::uint32_t h_em_nan = (h_e_mask | m_nan);
  const std::uint32_t h_e_norm_overflow_offset = (f_e_half_bias + 1);
  const std::uint32_t h_e_norm_overflow = (h_e_norm_overflow_offset << h_e_pos);
  const std::uint32_t h_e_norm = (f_e_half_bias << h_e_pos);
  const std::uint32_t h_m_norm = (f_m_rounded >> f_h_m_pos_offset);
  const std::uint32_t h_em_norm = (h_e_norm | h_m_norm);
  const std::uint32_t is_h_ndenorm_msb = (f_h_bias_offset - f_e_amount);
  const std::uint32_t is_f_e_flagged_msb = (f_h_e_biased_flag - f_e_half_bias);
  const std::uint32_t is_h_denorm_msb = (~is_h_ndenorm_msb);
  const std::uint32_t is_f_m_eqz_msb = (f_m - 1);
  const std::uint32_t is_h_nan_eqz_msb = (m_nan - 1);
  const std::uint32_t is_f_inf_msb = (is_f_e_flagged_msb & is_f_m_eqz_msb);
  const std::uint32_t is_f_nan_underflow_msb =
      (is_f_e_flagged_msb & is_h_nan_eqz_msb);
  const std::uint32_t is_e_overflow_msb = (h_e_mask_value - f_e_half_bias);
  const std::uint32_t is_h_inf_msb = (is_e_overflow_msb | is_f_inf_msb);
  const std::uint32_t is_f_nsnan_msb = (f_snan - f_snan_mask);
  const std::uint32_t is_m_norm_overflow_msb = (-f_m_rounded_overflow);
  const std::uint32_t is_f_snan_msb = (~is_f_nsnan_msb);
  const std::uint32_t h_em_overflow_result =
      _uint32_sels(is_m_norm_overflow_msb, h_e_norm_overflow, h_em_norm);
  const std::uint32_t h_em_nan_result =
      _uint32_sels(is_f_e_flagged_msb, h_em_nan, h_em_overflow_result);
  const std::uint32_t h_em_nan_underflow_result =
      _uint32_sels(is_f_nan_underflow_msb, h_nan_min, h_em_nan_result);
  const std::uint32_t h_em_inf_result =
      _uint32_sels(is_h_inf_msb, h_e_mask, h_em_nan_underflow_result);
  const std::uint32_t h_em_denorm_result =
      _uint32_sels(is_h_denorm_msb, h_m_denorm, h_em_inf_result);
  const std::uint32_t h_em_snan_result =
      _uint32_sels(is_f_snan_msb, h_snan_mask, h_em_denorm_result);
  const std::uint32_t h_result = (h_s | h_em_snan_result);
  return (std::uint16_t)(h_result);
}

} // namespace CesiumForUnityNative
